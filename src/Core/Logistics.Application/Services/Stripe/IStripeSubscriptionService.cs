using Logistics.Domain.Entities;
using Stripe;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Application.Services;

public interface IStripeSubscriptionService
{
    /// <summary>
    ///     Create a new subscription in Stripe for the given plan and customer.
    /// </summary>
    Task<StripeSubscription> CreateSubscriptionAsync(SubscriptionPlan plan, Tenant tenant, int truckCount,
        bool trial = false);

    /// <summary>
    ///     Cancels a subscription immediately or at period end.
    /// </summary>
    Task<StripeSubscription> CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelImmediately = true);

    /// <summary>
    ///     Update the quantity of the subscription in Stripe.
    /// </summary>
    Task<SubscriptionItem> UpdateSubscriptionQuantityAsync(string stripeSubscriptionId, int truckCount);

    /// <summary>
    ///     Renew an existing subscription or create a new one if it doesn't exist in Stripe.
    /// </summary>
    Task<StripeSubscription> RenewSubscriptionAsync(
        Subscription? subEntity,
        SubscriptionPlan plan,
        Tenant tenant,
        int truckCount);

    /// <summary>
    ///     Change a subscription's plan by swapping Stripe line items with proration.
    /// </summary>
    Task<StripeSubscription> ChangeSubscriptionPlanAsync(
        string stripeSubscriptionId, SubscriptionPlan newPlan, int truckCount);

    /// <summary>
    ///     Create a new subscription plan in Stripe.
    /// </summary>
    Task<(Product Product, Price BasePrice, Price PerTruckPrice)> CreateSubscriptionPlanAsync(SubscriptionPlan plan);

    /// <summary>
    ///     Update an existing subscription plan in Stripe.
    /// </summary>
    Task<(Product Product, Price ActiveBasePrice, Price ActivePerTruckPrice)> UpdateSubscriptionPlanAsync(
        SubscriptionPlan plan);
}
