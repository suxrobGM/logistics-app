using Logistics.Application.Abstractions.Payments;
using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Stripe;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
/// Manages customer subscriptions in Stripe (create, cancel, renew, change plan).
/// </summary>
internal sealed class StripeSubscriptionService(ILogger<StripeSubscriptionService> logger) : IStripeSubscriptionService
{
    public async Task<StripeSubscriptionInfo> CreateSubscriptionAsync(
        SubscriptionPlan plan, Tenant tenant, int truckCount, int? trialDays = null)
    {
        if (tenant.StripeCustomerId is null)
            throw new ArgumentException("Tenant must have a StripeCustomerId");

        var createOptions = new SubscriptionCreateOptions
        {
            Customer = tenant.StripeCustomerId,
            Items = BuildSubscriptionItems(plan, truckCount),
            Metadata = new Dictionary<string, string>
            {
                { StripeMetadataKeys.TenantId, tenant.Id.ToString() },
                { StripeMetadataKeys.PlanId, plan.Id.ToString() }
            }
        };

        if (trialDays.HasValue && trialDays.Value > 0)
        {
            // The portal never collects a card, so the trial must start without one and
            // pause - not error - when it ends card-less.
            createOptions.TrialEnd = DateTime.UtcNow.AddDays(trialDays.Value);
            createOptions.TrialSettings = new SubscriptionTrialSettingsOptions
            {
                EndBehavior = new SubscriptionTrialSettingsEndBehaviorOptions
                {
                    MissingPaymentMethod = "pause"
                }
            };
        }

        var subscription = await new SubscriptionService().CreateAsync(createOptions);
        logger.LogInformation("Created Stripe subscription for tenant {TenantId}", tenant.Id);
        return subscription.ToInfo();
    }

    public async Task<StripeSubscriptionInfo> CancelSubscriptionAsync(
        string stripeSubscriptionId, bool cancelImmediately = true)
    {
        var service = new SubscriptionService();

        if (cancelImmediately)
        {
            var cancelled = await service.CancelAsync(stripeSubscriptionId, new SubscriptionCancelOptions
            {
                InvoiceNow = true,
                Prorate = true
            });
            logger.LogInformation("Canceled immediately Stripe subscription {SubscriptionId}", stripeSubscriptionId);
            return cancelled.ToInfo();
        }

        var updated = await service.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        });
        logger.LogInformation("Canceled at period end Stripe subscription {SubscriptionId}", stripeSubscriptionId);
        return updated.ToInfo();
    }

    public Task<StripeSubscriptionInfo> RenewSubscriptionAsync(
        Subscription? subEntity, SubscriptionPlan plan, Tenant tenant, int truckCount) =>
        StripeApi.CallAsync(() => RenewAsync(subEntity, plan, tenant, truckCount));

    private async Task<StripeSubscriptionInfo> RenewAsync(
        Subscription? subEntity, SubscriptionPlan plan, Tenant tenant, int truckCount)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
            throw new ArgumentException("Tenant must have a StripeCustomerId");

        // Never had a Stripe subscription → create new
        if (subEntity is null || string.IsNullOrEmpty(subEntity.StripeSubscriptionId))
        {
            logger.LogInformation("Tenant {TenantId} creating first subscription", tenant.Id);
            return await CreateSubscriptionAsync(plan, tenant, truckCount);
        }

        var subSvc = new SubscriptionService();
        var stripeSub = await subSvc.GetAsync(subEntity.StripeSubscriptionId);

        // Scheduled to cancel but not yet → reactivate
        if (stripeSub.CancelAtPeriodEnd && stripeSub.Status != "canceled")
        {
            var updateOptions = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false
            };

            // Clear trial if it conflicts with the current period
            if (stripeSub.TrialEnd.HasValue && stripeSub.TrialEnd.Value > DateTime.UtcNow)
            {
                updateOptions.TrialEnd = SubscriptionTrialEnd.Now;
            }

            var reactivated = await subSvc.UpdateAsync(stripeSub.Id, updateOptions);
            logger.LogInformation("Tenant {TenantId} reactivated subscription", tenant.Id);
            return reactivated.ToInfo();
        }

        // Already canceled → start fresh
        if (stripeSub.Status is "canceled" or "incomplete_expired")
        {
            logger.LogInformation("Subscription {SubId} is canceled, creating new", stripeSub.Id);
            return await CreateSubscriptionAsync(plan, tenant, truckCount);
        }

        // Still active → bill immediately
        var invSvc = new InvoiceService();
        var invoice = await invSvc.CreateAsync(new InvoiceCreateOptions
        {
            Customer = tenant.StripeCustomerId,
            Subscription = stripeSub.Id,
            AutoAdvance = true
        });

        if (invoice.Status == "draft")
            await invSvc.FinalizeInvoiceAsync(invoice.Id);

        return stripeSub.ToInfo();
    }

    public async Task<StripeSubscriptionInfo> ChangeSubscriptionPlanAsync(
        string stripeSubscriptionId, SubscriptionPlan newPlan, int truckCount)
    {
        var subSvc = new SubscriptionService();
        var stripeSub = await subSvc.GetAsync(stripeSubscriptionId);

        // Remove existing items, add new plan's items
        var items = stripeSub.Items.Data
            .Select(existing => new SubscriptionItemOptions { Id = existing.Id, Deleted = true })
            .ToList();

        items.AddRange(BuildSubscriptionItems(newPlan, truckCount));

        var updated = await subSvc.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
        {
            Items = items,
            ProrationBehavior = "create_prorations",
            Metadata = new Dictionary<string, string>
            {
                { StripeMetadataKeys.PlanId, newPlan.Id.ToString() }
            }
        });

        logger.LogInformation("Changed subscription {SubscriptionId} to plan {PlanId} with {TruckCount} trucks",
            stripeSubscriptionId, newPlan.Id, truckCount);
        return updated.ToInfo();
    }

    public async Task<int> SyncAIOverageItemAsync(SubscriptionPlan plan)
    {
        if (string.IsNullOrEmpty(plan.StripePriceId) || string.IsNullOrEmpty(plan.StripeAIOveragePriceId))
            return 0;

        var subSvc = new SubscriptionService();
        var updatedCount = 0;

        // The base price identifies the plan's subscribers; their overage item may be absent or stale.
        var subscriptions = subSvc.ListAutoPagingAsync(new SubscriptionListOptions
        {
            Price = plan.StripePriceId,
            Status = "active"
        });

        await foreach (var subscription in subscriptions)
        {
            if (subscription.Items.Data.Any(i => i.Price.Id == plan.StripeAIOveragePriceId))
                continue;

            var items = subscription.Items.Data
                .Where(i => i.Price.Recurring?.UsageType == "metered")
                .Select(i => new SubscriptionItemOptions { Id = i.Id, Deleted = true })
                .ToList();
            items.Add(new SubscriptionItemOptions { Price = plan.StripeAIOveragePriceId });

            await subSvc.UpdateAsync(subscription.Id, new SubscriptionUpdateOptions
            {
                Items = items,
                ProrationBehavior = "none"
            });
            updatedCount++;
            logger.LogInformation(
                "Synced AI overage price {PriceId} onto subscription {SubscriptionId}",
                plan.StripeAIOveragePriceId, subscription.Id);
        }

        return updatedCount;
    }

    private static List<SubscriptionItemOptions> BuildSubscriptionItems(SubscriptionPlan plan, int truckCount)
    {
        var items = new List<SubscriptionItemOptions>
        {
            new() { Price = plan.StripePriceId, Quantity = 1 },
            new() { Price = plan.StripePerTruckPriceId, Quantity = truckCount }
        };

        if (!string.IsNullOrEmpty(plan.StripeAIOveragePriceId))
            items.Add(new SubscriptionItemOptions { Price = plan.StripeAIOveragePriceId });

        return items;
    }
}
