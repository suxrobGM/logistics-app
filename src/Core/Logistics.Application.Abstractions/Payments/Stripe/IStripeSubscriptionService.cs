using Logistics.Domain.Entities;

namespace Logistics.Application.Abstractions.Payments.Stripe;

public interface IStripeSubscriptionService
{
    Task<StripeSubscriptionInfo> CreateSubscriptionAsync(SubscriptionPlan plan, Tenant tenant, int truckCount,
        int? trialDays = null);

    Task<StripeSubscriptionInfo> CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelImmediately = true);

    /// <summary>
    /// Undoes a pending cancellation, creates a new subscription if there is none or Stripe has ended it, and
    /// otherwise bills the active one now.
    /// </summary>
    /// <exception cref="StripeApiException">Stripe rejected the renewal, for example for a missing payment method.</exception>
    Task<StripeSubscriptionInfo> RenewSubscriptionAsync(
        Subscription? subEntity, SubscriptionPlan plan, Tenant tenant, int truckCount);

    Task<StripeSubscriptionInfo> ChangeSubscriptionPlanAsync(
        string stripeSubscriptionId, SubscriptionPlan newPlan, int truckCount);

    /// <summary>Updates every active subscription on the plan to its current AI overage price; returns how many it changed.</summary>
    Task<int> SyncAIOverageItemAsync(SubscriptionPlan plan);
}
