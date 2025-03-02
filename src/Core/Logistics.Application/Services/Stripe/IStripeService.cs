using Logistics.Domain.Entities;
using StripeCustomer = Stripe.Customer;
using StripeSubscription = Stripe.Subscription;

namespace Logistics.Application.Services;

public interface IStripeService
{
    /// <summary>
    /// Create a new customer in Stripe for the given tenant.
    /// </summary>
    /// <param name="tenant">Tenant for which the customer is to be created.</param>
    /// <returns>Stripe customer object.</returns>
    Task<StripeCustomer> CreateCustomerAsync(Tenant tenant);
    
    /// <summary>
    /// Create a new subscription in Stripe for the given plan and customer.
    /// </summary>
    /// <param name="plan">Subscription plan.</param>
    /// <param name="stripeCustomerId">Stripe customer ID.</param>
    /// <param name="employeeCount">Number of employees in the tenant company to calculate the subscription price.</param>
    /// <returns>Stripe subscription object.</returns>
    Task<StripeSubscription> CreateSubscriptionAsync(SubscriptionPlan plan, string stripeCustomerId, int employeeCount);
    
    /// <summary>
    /// Update the quantity of the subscription in Stripe.
    /// </summary>
    /// <param name="stripeSubscriptionId">Stripe subscription ID.</param>
    /// <param name="employeeCount">New number of employees in the tenant company.</param>
    Task UpdateSubscriptionQuantityAsync(string stripeSubscriptionId, int employeeCount);
}