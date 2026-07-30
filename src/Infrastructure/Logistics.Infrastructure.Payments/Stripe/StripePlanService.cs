using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Stripe;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
/// Manages subscription plan products and prices in Stripe.
/// </summary>
internal sealed class StripePlanService(
    ISystemSettingsService settingService,
    ILogger<StripePlanService> logger) : IStripePlanService
{
    /// <summary>
    /// $0.10 per billing unit (standard=1 unit, premium=2). Changes propagate on the next
    /// UpdatePlanAsync/seeder run.
    /// </summary>
    private const decimal AIOverageUnitAmountCents = 10;

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

        logger.LogInformation("Created Stripe product for plan {PlanId}", plan.Id);

        // 2. Create base price
        var basePrice = await CreateLicensedPriceAsync(priceService, product.Id, plan,
            plan.Price * 100, plan.Price.Currency, "base");

        // 3. Create per-truck price
        var perTruckPrice = await CreateLicensedPriceAsync(priceService, product.Id, plan,
            plan.PerTruckPrice * 100, plan.PerTruckPrice.Currency, "per_truck");

        // 4. Create AI overage metered price if plan has a weekly quota
        Price? aiOveragePrice = null;
        if (plan.WeeklyAIRequestQuota.HasValue)
        {
            aiOveragePrice = await CreateMeteredOveragePriceAsync(priceService, product.Id, plan);
        }

        logger.LogInformation("Created Stripe prices for plan {PlanId}", plan.Id);
        return new StripePlanResult(product, basePrice, perTruckPrice, aiOveragePrice);
    }

    public async Task<StripePlanResult> SyncPlanAsync(SubscriptionPlan plan)
    {
        if (!string.IsNullOrEmpty(plan.StripeProductId))
        {
            try
            {
                return await UpdatePlanAsync(plan);
            }
            catch (StripeException ex) when (IsMissingResource(ex))
            {
                // The stored ids belong to a Stripe account or test dataset that no longer has
                // them. Rebuilding is the only way forward, and is safe: nothing can be billing
                // against a product Stripe cannot find.
                logger.LogWarning(
                    "Stripe product {ProductId} for plan {PlanId} no longer exists ({Message}); recreating",
                    plan.StripeProductId, plan.Id, ex.StripeError?.Message ?? ex.Message);

                plan.StripeProductId = null;
                plan.StripePriceId = null;
                plan.StripePerTruckPriceId = null;
                plan.StripeAIOveragePriceId = null;
            }
        }

        var created = await CreatePlanAsync(plan);
        plan.StripeProductId = created.Product.Id;
        plan.StripePriceId = created.BasePrice.Id;
        plan.StripePerTruckPriceId = created.PerTruckPrice.Id;
        plan.StripeAIOveragePriceId = created.AIOveragePrice?.Id;
        return created;
    }

    private static bool IsMissingResource(StripeException ex) =>
        ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound
        || ex.StripeError?.Code == "resource_missing";

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

        var activeOveragePrice = await ReconcileOveragePriceAsync(priceService, plan);
        if (activeOveragePrice is not null && activeOveragePrice.Id != plan.StripeAIOveragePriceId)
            plan.StripeAIOveragePriceId = activeOveragePrice.Id;

        return new StripePlanResult(product, activeBasePrice, activePerTruckPrice, activeOveragePrice);
    }

    /// <summary>
    /// Creates the metered AI overage price when missing and deactivate-recreates it on amount or
    /// interval change (Stripe prices are immutable). Swapping subscription items is the caller's
    /// job (SyncAIOverageItemAsync).
    /// </summary>
    private async Task<Price?> ReconcileOveragePriceAsync(PriceService priceService, SubscriptionPlan plan)
    {
        if (!plan.WeeklyAIRequestQuota.HasValue)
            return null;

        if (string.IsNullOrEmpty(plan.StripeAIOveragePriceId))
            return await CreateMeteredOveragePriceAsync(priceService, plan.StripeProductId!, plan);

        var existing = await priceService.GetAsync(plan.StripeAIOveragePriceId);
        if (!HasDrifted(existing, plan, AIOverageUnitAmountCents, plan.Price.Currency))
            return existing;

        var newPrice = await CreateMeteredOveragePriceAsync(priceService, plan.StripeProductId!, plan);
        if (newPrice is null)
            return existing;

        await priceService.UpdateAsync(existing.Id, new PriceUpdateOptions { Active = false });
        logger.LogInformation("Recreated Stripe AI overage price for plan {PlanId} ({OldPriceId} -> {NewPriceId})",
            plan.Id, existing.Id, newPrice.Id);
        return newPrice;
    }

    /// <summary>
    /// Whether a live Stripe price still matches the plan. Stripe prices are immutable, so a
    /// mismatch means the price has to be recreated rather than edited.
    /// </summary>
    private static bool HasDrifted(
        Price existing, SubscriptionPlan plan, decimal expectedAmountCents, string expectedCurrency)
    {
        if (existing.UnitAmountDecimal != expectedAmountCents)
            return true;

        if (!existing.Currency.Equals(expectedCurrency, StringComparison.OrdinalIgnoreCase))
            return true;

        return !existing.Recurring.Interval.Equals(plan.Interval.ToString(), StringComparison.OrdinalIgnoreCase)
               || existing.Recurring.IntervalCount != plan.IntervalCount;
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
        var meterId = await settingService.GetAsync(StripeSettingKeys.AIOverageMeterId);
        if (string.IsNullOrEmpty(meterId))
        {
            logger.LogWarning("Cannot create AI overage price: billing meter not configured");
            return null;
        }

        var price = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = productId,
            UnitAmountDecimal = AIOverageUnitAmountCents,
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

        logger.LogInformation("Created Stripe AI overage metered price for plan {PlanId}", plan.Id);
        return price;
    }

    private async Task<Price> RecreateIfChangedAsync(
        PriceService priceService, SubscriptionPlan plan, string existingPriceId,
        decimal expectedAmountCents, string expectedCurrency, string priceType)
    {
        var existing = await priceService.GetAsync(existingPriceId);
        if (!HasDrifted(existing, plan, expectedAmountCents, expectedCurrency))
            return existing;

        var newPrice = await CreateLicensedPriceAsync(
            priceService, plan.StripeProductId!, plan, expectedAmountCents, expectedCurrency, priceType);
        logger.LogInformation("Recreated Stripe {PriceType} price for plan {PlanId}", priceType, plan.Id);
        return newPrice;
    }

}
