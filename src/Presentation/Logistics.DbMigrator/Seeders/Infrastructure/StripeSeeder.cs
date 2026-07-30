using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Stripe;
using Stripe.Billing;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Infrastructure.Payments.Stripe;
using Microsoft.Extensions.Options;

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

    /// <summary>Label only; the meter's identity is its event name, which comes from config.</summary>
    private const string MeterDisplayName = "AI Agent Sessions (Dispatch & Copilot)";

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

        var stripeOptions = context.ServiceProvider.GetRequiredService<IOptions<StripeOptions>>().Value;

        // 1. Ensure billing meter exists (one-time setup, stored in SystemSettings)
        await EnsureBillingMeterAsync(settingService, stripeOptions.AIOverageMeterEventName, ct);

        // 2. Create products/prices for new plans; reconcile already-synced plans so price
        //    changes propagate.
        var plans = await planRepo.GetListAsync(ct: ct);
        var syncedCount = 0;

        foreach (var plan in plans)
        {
            // SyncPlanAsync writes the resulting ids onto the plan itself.
            var previousOveragePriceId = plan.StripeAIOveragePriceId;
            await planService.SyncPlanAsync(plan);
            await context.MasterUnitOfWork.SaveChangesAsync(ct);

            if (plan.StripeAIOveragePriceId != previousOveragePriceId)
            {
                var swapped = await subscriptionService.SyncAIOverageItemAsync(plan);
                logger.LogInformation(
                    "Reconciled AI overage price for plan '{PlanName}' ({Swapped} subscriptions updated)",
                    plan.Name, swapped);
            }

            syncedCount++;
            logger.LogInformation("Synced plan '{PlanName}' to Stripe (product: {ProductId})",
                plan.Name, plan.StripeProductId);
        }

        LogCompleted(syncedCount);
    }

    /// <summary>
    /// Resolves by event name every run rather than trusting the stored id, which can outlive what
    /// it points at. A meter that does not match the reported event name bills nothing, silently.
    /// </summary>
    private async Task EnsureBillingMeterAsync(
        ISystemSettingsService settingService, string eventName, CancellationToken ct)
    {
        var meterService = new MeterService();
        var meters = await meterService.ListAsync(new MeterListOptions { Limit = 100 }, cancellationToken: ct);
        var existingMeter = meters.Data
            .FirstOrDefault(m => m.EventName == eventName && m.Status == "active");

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
                DisplayName = MeterDisplayName,
                EventName = eventName,
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
            "Stripe Billing Meter ID for AI agent session overages (dispatch and copilot)", ct);
    }
}
