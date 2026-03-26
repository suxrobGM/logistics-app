using Logistics.Domain.Entities;
using Stripe;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Application.Services;

/// <summary>
/// Manages customer subscriptions in Stripe (create, cancel, renew, change plan).
/// </summary>
public interface IStripeSubscriptionService
{
    Task<StripeSubscription> CreateSubscriptionAsync(SubscriptionPlan plan, Tenant tenant, int truckCount,
        bool trial = false);

    Task<StripeSubscription> CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelImmediately = true);

    Task<SubscriptionItem> UpdateSubscriptionQuantityAsync(string stripeSubscriptionId, int truckCount);

    Task<StripeSubscription> RenewSubscriptionAsync(
        Subscription? subEntity, SubscriptionPlan plan, Tenant tenant, int truckCount);

    Task<StripeSubscription> ChangeSubscriptionPlanAsync(
        string stripeSubscriptionId, SubscriptionPlan newPlan, int truckCount);
}
