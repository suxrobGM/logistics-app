using Logistics.Application.Services;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Stripe;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Syncs Stripe subscriptions with the local database.
/// Handles cases where Stripe webhooks were missed (e.g., Stripe CLI wasn't running).
/// </summary>
internal class StripeSubscriptionSyncSeeder(ILogger<StripeSubscriptionSyncSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(StripeSubscriptionSyncSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 36; // After StripeSeeder (35)
    public override IReadOnlyList<string> DependsOn => [nameof(StripeSeeder)];

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

        var masterUow = context.MasterUnitOfWork;
        var subscriptionService = new SubscriptionService();
        var stripeSubscriptions = await subscriptionService.ListAsync(new SubscriptionListOptions
        {
            Limit = 100
        }, cancellationToken: ct);

        var syncedCount = 0;

        foreach (var stripeSub in stripeSubscriptions.Data)
        {
            if (await SyncSubscriptionAsync(stripeSub, masterUow, ct))
            {
                syncedCount++;
            }
        }

        if (syncedCount > 0)
        {
            await masterUow.SaveChangesAsync(ct);
        }

        LogCompleted(syncedCount);
    }

    private async Task<bool> SyncSubscriptionAsync(
        StripeSubscription stripeSub,
        IMasterUnitOfWork masterUow,
        CancellationToken ct)
    {
        if (!stripeSub.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantIdStr)
            || !Guid.TryParse(tenantIdStr, out var tenantId))
        {
            logger.LogWarning("Skipping Stripe subscription {SubscriptionId}: missing or invalid tenant_id metadata",
                stripeSub.Id);
            return false;
        }

        if (!stripeSub.Metadata.TryGetValue(StripeMetadataKeys.PlanId, out var planIdStr)
            || !Guid.TryParse(planIdStr, out var planId))
        {
            logger.LogWarning("Skipping Stripe subscription {SubscriptionId}: missing or invalid plan_id metadata",
                stripeSub.Id);
            return false;
        }

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            logger.LogWarning("Skipping Stripe subscription {SubscriptionId}: tenant {TenantId} not found",
                stripeSub.Id, tenantId);
            return false;
        }

        var plan = await masterUow.Repository<SubscriptionPlan>().GetByIdAsync(planId, ct);
        if (plan is null)
        {
            logger.LogWarning("Skipping Stripe subscription {SubscriptionId}: plan {PlanId} not found",
                stripeSub.Id, planId);
            return false;
        }

        var status = StripeObjectMapper.GetSubscriptionStatus(stripeSub.Status);
        var subscriptionRepo = masterUow.Repository<Subscription>();

        // Check if subscription already exists locally by Stripe ID
        var existing = await subscriptionRepo.GetAsync(
            s => s.StripeSubscriptionId == stripeSub.Id, ct);

        if (existing is not null)
        {
            if (existing.Status == status
                && existing.PlanId == planId
                && existing.CancelAtPeriodEnd == stripeSub.CancelAtPeriodEnd)
            {
                logger.LogInformation("Subscription {SubscriptionId} already in sync", stripeSub.Id);
                return false;
            }

            existing.Status = status;
            existing.PlanId = planId;
            existing.CancelAtPeriodEnd = stripeSub.CancelAtPeriodEnd;
            subscriptionRepo.Update(existing);

            logger.LogInformation("Updated subscription {SubscriptionId} for tenant {TenantName}",
                stripeSub.Id, tenant.Name);
            return true;
        }

        // Tenant already has a subscription with a different Stripe ID — relink it
        if (tenant.Subscription is not null)
        {
            tenant.Subscription.Status = status;
            tenant.Subscription.StripeSubscriptionId = stripeSub.Id;
            tenant.Subscription.PlanId = planId;
            tenant.Subscription.CancelAtPeriodEnd = stripeSub.CancelAtPeriodEnd;
            subscriptionRepo.Update(tenant.Subscription);

            logger.LogInformation("Relinked subscription {SubscriptionId} to tenant {TenantName}",
                stripeSub.Id, tenant.Name);
            return true;
        }

        // Create new subscription
        var subscription = new Subscription
        {
            Status = status,
            TenantId = tenant.Id,
            Tenant = tenant,
            PlanId = plan.Id,
            Plan = plan,
            StripeSubscriptionId = stripeSub.Id,
            StripeCustomerId = tenant.StripeCustomerId,
            CancelAtPeriodEnd = stripeSub.CancelAtPeriodEnd
        };

        await subscriptionRepo.AddAsync(subscription, ct);
        logger.LogInformation("Created subscription {SubscriptionId} for tenant {TenantName}",
            stripeSub.Id, tenant.Name);
        return true;
    }
}
