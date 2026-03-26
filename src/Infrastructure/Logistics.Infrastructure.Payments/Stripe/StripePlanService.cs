using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
/// Manages subscription plan products and prices in Stripe.
/// </summary>
internal sealed class StripePlanService(
    IOptions<StripeOptions> options,
    ISystemSettingService settingService,
    ILogger<StripePlanService> logger) : StripeServiceBase(options, logger), IStripePlanService
{
    private const string MeterSettingKey = "Stripe:AiOverageMeterId";

    public async Task<StripePlanResult> CreatePlanAsync(SubscriptionPlan plan)
    {
        var productService = new ProductService();
        var priceService = new PriceService();

        // 1. Create the Product
        var product = await productService.CreateAsync(new ProductCreateOptions
        {
            Name = plan.Name,
            Description = plan.Description,
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.PlanId] = plan.Id.ToString()
            }
        });

        Logger.LogInformation("Created Stripe product for plan {PlanId}", plan.Id);

        // 2. Create base price
        var basePrice = await CreateLicensedPriceAsync(priceService, product.Id, plan,
            plan.Price * 100, plan.Price.Currency, "base");

        // 3. Create per-truck price
        var perTruckPrice = await CreateLicensedPriceAsync(priceService, product.Id, plan,
            plan.PerTruckPrice * 100, plan.PerTruckPrice.Currency, "per_truck");

        // 4. Apply billing cycle anchor metadata if configured
        if (plan.BillingCycleAnchor.HasValue)
        {
            var anchor = plan.BillingCycleAnchor.Value.ToString("O");
            await UpdatePriceMetadataAsync(priceService, basePrice.Id, anchor, "base");
            await UpdatePriceMetadataAsync(priceService, perTruckPrice.Id, anchor, "per_truck");
        }

        // 5. Create AI overage metered price if plan has a weekly quota
        Price? aiOveragePrice = null;
        if (plan.WeeklyAiSessionQuota.HasValue)
        {
            aiOveragePrice = await CreateMeteredOveragePriceAsync(priceService, product.Id, plan);
        }

        Logger.LogInformation("Created Stripe prices for plan {PlanId}", plan.Id);
        return new StripePlanResult(product, basePrice, perTruckPrice, aiOveragePrice);
    }

    public async Task<StripePlanResult> UpdatePlanAsync(SubscriptionPlan plan)
    {
        if (string.IsNullOrEmpty(plan.StripeProductId))
            throw new ArgumentException("SubscriptionPlan must have a StripeProductId");
        if (string.IsNullOrEmpty(plan.StripePriceId))
            throw new ArgumentException("SubscriptionPlan must have a StripePriceId");

        var productService = new ProductService();
        var priceService = new PriceService();

        // Update product
        var product = await productService.UpdateAsync(plan.StripeProductId, new ProductUpdateOptions
        {
            Name = plan.Name,
            Description = plan.Description,
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.PlanId] = plan.Id.ToString()
            }
        });

        // Check if base price needs recreation
        var activeBasePrice = await RecreateIfChangedAsync(priceService, plan, plan.StripePriceId,
            plan.Price * 100, plan.Price.Currency, "base");
        if (activeBasePrice.Id != plan.StripePriceId)
            plan.StripePriceId = activeBasePrice.Id;

        // Check if per-truck price needs recreation
        Price activePerTruckPrice;
        if (!string.IsNullOrEmpty(plan.StripePerTruckPriceId))
        {
            activePerTruckPrice = await RecreateIfChangedAsync(priceService, plan, plan.StripePerTruckPriceId,
                plan.PerTruckPrice * 100, plan.PerTruckPrice.Currency, "per_truck");
            if (activePerTruckPrice.Id != plan.StripePerTruckPriceId)
                plan.StripePerTruckPriceId = activePerTruckPrice.Id;
        }
        else
        {
            activePerTruckPrice = await CreateLicensedPriceAsync(priceService, plan.StripeProductId, plan,
                plan.PerTruckPrice * 100, plan.PerTruckPrice.Currency, "per_truck");
            plan.StripePerTruckPriceId = activePerTruckPrice.Id;
        }

        return new StripePlanResult(product, activeBasePrice, activePerTruckPrice);
    }

    private async Task<Price> CreateLicensedPriceAsync(
        PriceService priceService, string productId, SubscriptionPlan plan,
        decimal unitAmountCents, string currency, string priceType)
    {
        return await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = productId,
            UnitAmountDecimal = unitAmountCents,
            Currency = currency.ToLower(),
            Recurring = new PriceRecurringOptions
            {
                Interval = plan.Interval.ToString().ToLower(),
                IntervalCount = plan.IntervalCount,
                UsageType = "licensed"
            },
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.PlanId] = plan.Id.ToString(),
                ["price_type"] = priceType
            }
        });
    }

    private async Task<Price?> CreateMeteredOveragePriceAsync(
        PriceService priceService, string productId, SubscriptionPlan plan)
    {
        var meterId = await settingService.GetAsync(MeterSettingKey);
        if (string.IsNullOrEmpty(meterId))
        {
            Logger.LogWarning("Cannot create AI overage price: billing meter not configured");
            return null;
        }

        var price = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = productId,
            UnitAmountDecimal = 40, // $0.40 per session
            Currency = plan.Price.Currency.ToLower(),
            Recurring = new PriceRecurringOptions
            {
                Interval = plan.Interval.ToString().ToLower(),
                IntervalCount = plan.IntervalCount,
                UsageType = "metered",
                Meter = meterId
            },
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.PlanId] = plan.Id.ToString(),
                ["price_type"] = "ai_overage"
            }
        });

        Logger.LogInformation("Created Stripe AI overage metered price for plan {PlanId}", plan.Id);
        return price;
    }

    private async Task<Price> RecreateIfChangedAsync(
        PriceService priceService, SubscriptionPlan plan, string existingPriceId,
        decimal expectedAmountCents, string expectedCurrency, string priceType)
    {
        var existing = await priceService.GetAsync(existingPriceId);
        var amountChanged = existing.UnitAmountDecimal != expectedAmountCents;
        var currencyChanged = !existing.Currency.Equals(expectedCurrency, StringComparison.OrdinalIgnoreCase);

        var billingChanged = !existing.Recurring.Interval.Equals(
            plan.Interval.ToString(), StringComparison.OrdinalIgnoreCase)
            || existing.Recurring.IntervalCount != plan.IntervalCount;

        if (!amountChanged && !currencyChanged && !billingChanged)
            return existing;

        var newPrice = await CreateLicensedPriceAsync(
            priceService, plan.StripeProductId!, plan, expectedAmountCents, expectedCurrency, priceType);
        Logger.LogInformation("Recreated Stripe {PriceType} price for plan {PlanId}", priceType, plan.Id);
        return newPrice;
    }

    private static async Task UpdatePriceMetadataAsync(
        PriceService priceService, string priceId, string billingAnchor, string priceType)
    {
        await priceService.UpdateAsync(priceId, new PriceUpdateOptions
        {
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.BillingCycleAnchor] = billingAnchor,
                ["price_type"] = priceType
            }
        });
    }
}
