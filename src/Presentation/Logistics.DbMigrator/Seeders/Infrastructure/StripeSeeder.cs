using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Stripe;
using Stripe.Billing;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Seeds Stripe resources: creates billing meter, products, and prices for subscription plans.
/// Skips if Stripe API key is not configured.
/// </summary>
internal class StripeSeeder(ILogger<StripeSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(StripeSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 35;
    public override IReadOnlyList<string> DependsOn => [nameof(SubscriptionPlanSeeder)];

    private const string MeterEventName = "ai_dispatch_session";

    public override Task<bool> ShouldSkipAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        var stripeKey = context.Configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(stripeKey))
        {
            LogSkipping("Stripe API key not configured");
            return Task.FromResult(true);
        }

        StripeConfiguration.ApiKey = stripeKey;
        return Task.FromResult(false);
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken ct = default)
    {
        LogStarting();

        var settingService = context.ServiceProvider.GetRequiredService<ISystemSettingsService>();
        var planService = context.ServiceProvider.GetRequiredService<IStripePlanService>();
        var subscriptionService = context.ServiceProvider.GetRequiredService<IStripeSubscriptionService>();
        var planRepo = context.MasterUnitOfWork.Repository<SubscriptionPlan>();

        // 1. Ensure billing meter exists (one-time setup, stored in SystemSettings)
        await EnsureBillingMeterAsync(settingService, ct);

        // 2. Create products/prices for new plans; reconcile already-synced plans so price
        //    changes propagate.
        var plans = await planRepo.GetListAsync(ct: ct);
        var syncedCount = 0;

        foreach (var plan in plans)
        {
            if (string.IsNullOrEmpty(plan.StripeProductId))
            {
                var created = await planService.CreatePlanAsync(plan);
                plan.StripeProductId = created.Product.Id;
                plan.StripePriceId = created.BasePrice.Id;
                plan.StripePerTruckPriceId = created.PerTruckPrice.Id;
                plan.StripeAIOveragePriceId = created.AIOveragePrice?.Id;

                await context.MasterUnitOfWork.SaveChangesAsync(ct);
                syncedCount++;
                logger.LogInformation("Synced plan '{PlanName}' to Stripe (product: {ProductId})",
                    plan.Name, plan.StripeProductId);
                continue;
            }

            // UpdatePlanAsync writes the refreshed price ids onto the plan itself.
            var previousOveragePriceId = plan.StripeAIOveragePriceId;
            await planService.UpdatePlanAsync(plan);
            await context.MasterUnitOfWork.SaveChangesAsync(ct);

            if (plan.StripeAIOveragePriceId != previousOveragePriceId)
            {
                var swapped = await subscriptionService.SyncAIOverageItemAsync(plan);
                logger.LogInformation(
                    "Reconciled AI overage price for plan '{PlanName}' ({Swapped} subscriptions updated)",
                    plan.Name, swapped);
            }

            syncedCount++;
        }

        LogCompleted(syncedCount);
    }

    private async Task EnsureBillingMeterAsync(ISystemSettingsService settingService, CancellationToken ct)
    {
        var existing = await settingService.GetAsync(StripeSettingKeys.AIOverageMeterId, ct);
        if (existing is not null)
        {
            logger.LogInformation("Billing meter already configured: {MeterId}", existing);
            return;
        }

        // Search for existing meter by event name
        var meterService = new MeterService();
        var meters = await meterService.ListAsync(cancellationToken: ct);
        var existingMeter = meters.Data.FirstOrDefault(m => m.EventName == MeterEventName);

        string meterId;
        if (existingMeter is not null)
        {
            meterId = existingMeter.Id;
            logger.LogInformation("Found existing Stripe billing meter: {MeterId}", meterId);
        }
        else
        {
            var meter = await meterService.CreateAsync(new MeterCreateOptions
            {
                DisplayName = "AI Dispatch Sessions",
                EventName = MeterEventName,
                DefaultAggregation = new MeterDefaultAggregationOptions { Formula = "sum" },
                CustomerMapping = new MeterCustomerMappingOptions
                {
                    EventPayloadKey = "stripe_customer_id",
                    Type = "by_id"
                },
                ValueSettings = new MeterValueSettingsOptions
                {
                    EventPayloadKey = "value"
                }
            }, cancellationToken: ct);

            meterId = meter.Id;
            logger.LogInformation("Created Stripe billing meter: {MeterId}", meterId);
        }

        await settingService.SetAsync(StripeSettingKeys.AIOverageMeterId, meterId,
            "Stripe Billing Meter ID for AI dispatch session overages", ct);
    }
}
