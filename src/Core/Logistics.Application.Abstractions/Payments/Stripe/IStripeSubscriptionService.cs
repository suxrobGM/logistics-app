using Logistics.Domain.Entities;

namespace Logistics.Application.Abstractions.Payments.Stripe;

public interface IStripeSubscriptionService
{
    Task<StripeSubscriptionInfo> CreateSubscriptionAsync(SubscriptionPlan plan, Tenant tenant, int truckCount,
        int? trialDays = null);

    Task<StripeSubscriptionInfo> CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelImmediately = true);

    /// <summary>Reactivates, recreates, or bills the subscription, whichever its Stripe state calls for.</summary>
    /// <exception cref="StripeApiException">Stripe rejected the renewal, for example for a missing payment method.</exception>
    Task<StripeSubscriptionInfo> RenewSubscriptionAsync(
        Subscription? subEntity, SubscriptionPlan plan, Tenant tenant, int truckCount);

    Task<StripeSubscriptionInfo> ChangeSubscriptionPlanAsync(
        string stripeSubscriptionId, SubscriptionPlan newPlan, int truckCount);

    /// <summary>Puts the plan's current AI overage price on each active subscription; returns how many changed.</summary>
    Task<int> SyncAIOverageItemAsync(SubscriptionPlan plan);
}
