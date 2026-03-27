using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
/// Manages customer subscriptions in Stripe (create, cancel, renew, change plan).
/// </summary>
internal sealed class StripeSubscriptionService(
    IOptions<StripeOptions> options,
    ILogger<StripeSubscriptionService> logger)
    : StripeServiceBase(options, logger), IStripeSubscriptionService
{
    public async Task<StripeSubscription> CreateSubscriptionAsync(
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
            createOptions.PaymentBehavior = "default_incomplete";
            createOptions.TrialEnd = DateTime.UtcNow.AddDays(trialDays.Value);
        }

        var subscription = await new SubscriptionService().CreateAsync(createOptions);
        Logger.LogInformation("Created Stripe subscription for tenant {TenantId}", tenant.Id);
        return subscription;
    }

    public async Task<StripeSubscription> CancelSubscriptionAsync(
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
            Logger.LogInformation("Canceled immediately Stripe subscription {SubscriptionId}", stripeSubscriptionId);
            return cancelled;
        }

        var updated = await service.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        });
        Logger.LogInformation("Canceled at period end Stripe subscription {SubscriptionId}", stripeSubscriptionId);
        return updated;
    }

    public async Task<SubscriptionItem> UpdateSubscriptionQuantityAsync(
        string stripeSubscriptionId, int truckCount)
    {
        var subscription = await new SubscriptionService().GetAsync(stripeSubscriptionId);
        var item = subscription.Items.Data.FirstOrDefault(i => i.Quantity != 1)
                   ?? subscription.Items.Data.Last();

        var result = await new SubscriptionItemService().UpdateAsync(item.Id,
            new SubscriptionItemUpdateOptions { Quantity = truckCount });
        Logger.LogInformation("Updated Stripe subscription {SubscriptionId} truck count to {TruckCount}",
            stripeSubscriptionId, truckCount);
        return result;
    }

    public async Task<StripeSubscription> RenewSubscriptionAsync(
        Subscription? subEntity, SubscriptionPlan plan, Tenant tenant, int truckCount)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
            throw new ArgumentException("Tenant must have a StripeCustomerId");

        // Never had a Stripe subscription → create new
        if (subEntity is null || string.IsNullOrEmpty(subEntity.StripeSubscriptionId))
        {
            Logger.LogInformation("Tenant {TenantId} creating first subscription", tenant.Id);
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
            Logger.LogInformation("Tenant {TenantId} reactivated subscription", tenant.Id);
            return reactivated;
        }

        // Already canceled → start fresh
        if (stripeSub.Status is "canceled" or "incomplete_expired")
        {
            Logger.LogInformation("Subscription {SubId} is canceled, creating new", stripeSub.Id);
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

        return stripeSub;
    }

    public async Task<StripeSubscription> ChangeSubscriptionPlanAsync(
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

        Logger.LogInformation("Changed subscription {SubscriptionId} to plan {PlanId} with {TruckCount} trucks",
            stripeSubscriptionId, newPlan.Id, truckCount);
        return updated;
    }

    private static List<SubscriptionItemOptions> BuildSubscriptionItems(SubscriptionPlan plan, int truckCount)
    {
        var items = new List<SubscriptionItemOptions>
        {
            new() { Price = plan.StripePriceId, Quantity = 1 },
            new() { Price = plan.StripePerTruckPriceId, Quantity = truckCount }
        };

        if (!string.IsNullOrEmpty(plan.StripeAiOveragePriceId))
            items.Add(new SubscriptionItemOptions { Price = plan.StripeAiOveragePriceId });

        return items;
    }
}
