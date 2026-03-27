using Logistics.Application.Services;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Stripe;
using Stripe.Billing;

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
    private const string MeterSettingKey = "Stripe:AiOverageMeterId";

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

        var settingService = context.ServiceProvider.GetRequiredService<ISystemSettingService>();
        var planService = context.ServiceProvider.GetRequiredService<IStripePlanService>();
        var planRepo = context.MasterUnitOfWork.Repository<SubscriptionPlan>();

        // 1. Ensure billing meter exists (one-time setup, stored in SystemSetting)
        await EnsureBillingMeterAsync(settingService, ct);

        // 2. Create Stripe products and prices for plans without Stripe IDs
        var plans = await planRepo.GetListAsync(ct: ct);
        var syncedCount = 0;

        foreach (var plan in plans)
        {
            if (!string.IsNullOrEmpty(plan.StripeProductId))
            {
                logger.LogInformation("Plan '{PlanName}' already synced to Stripe", plan.Name);
                continue;
            }

            var result = await planService.CreatePlanAsync(plan);
            plan.StripeProductId = result.Product.Id;
            plan.StripePriceId = result.BasePrice.Id;
            plan.StripePerTruckPriceId = result.PerTruckPrice.Id;
            plan.StripeAiOveragePriceId = result.AiOveragePrice?.Id;

            await context.MasterUnitOfWork.SaveChangesAsync(ct);
            syncedCount++;
            logger.LogInformation("Synced plan '{PlanName}' to Stripe (product: {ProductId})",
                plan.Name, plan.StripeProductId);
        }

        LogCompleted(syncedCount);
    }

    private async Task EnsureBillingMeterAsync(ISystemSettingService settingService, CancellationToken ct)
    {
        var existing = await settingService.GetAsync(MeterSettingKey, ct);
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

        await settingService.SetAsync(MeterSettingKey, meterId,
            "Stripe Billing Meter ID for AI dispatch session overages", ct);
    }
}
