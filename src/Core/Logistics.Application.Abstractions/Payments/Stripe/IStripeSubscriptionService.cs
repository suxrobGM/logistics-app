using Logistics.Domain.Entities;

namespace Logistics.Application.Abstractions.Payments.Stripe;

/// <summary>
/// Manages customer subscriptions in Stripe (create, cancel, renew, change plan).
/// </summary>
public interface IStripeSubscriptionService
{
    Task<StripeSubscriptionInfo> CreateSubscriptionAsync(SubscriptionPlan plan, Tenant tenant, int truckCount,
        int? trialDays = null);

    Task<StripeSubscriptionInfo> CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelImmediately = true);

    /// <summary>
    /// Reactivates, recreates, or bills the subscription, whichever its Stripe state calls for.
    /// </summary>
    /// <exception cref="StripeApiException">Stripe rejected the renewal, for example for a missing payment method.</exception>
    Task<StripeSubscriptionInfo> RenewSubscriptionAsync(
        Subscription? subEntity, SubscriptionPlan plan, Tenant tenant, int truckCount);

    Task<StripeSubscriptionInfo> ChangeSubscriptionPlanAsync(
        string stripeSubscriptionId, SubscriptionPlan newPlan, int truckCount);

    /// <summary>
    /// Ensures every active subscription on the plan carries its current metered AI overage
    /// price - adds the missing item or replaces a stale one. Returns subscriptions updated.
    /// </summary>
    Task<int> SyncAIOverageItemAsync(SubscriptionPlan plan);
}
