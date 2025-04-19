using Logistics.Domain.Entities;
using Stripe;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;
using StripeCustomer = Stripe.Customer;
using StripeSubscription = Stripe.Subscription;
using StripePaymentMethod = Stripe.PaymentMethod;

namespace Logistics.Application.Services;

public interface IStripeService
{
    /// <summary>
    /// Retrieve a Stripe customer by their ID.
    /// </summary>
    /// <param name="stripeCustomerId">Stripe customer ID.</param>
    /// <returns>Stripe customer object.</returns>
    Task<StripeCustomer> GetCustomerAsync(string stripeCustomerId);
    
    /// <summary>
    /// Create a new customer in Stripe for the given tenant.
    /// </summary>
    /// <param name="tenant">Tenant for which the customer is to be created.</param>
    /// <returns>Stripe customer object.</returns>
    Task<StripeCustomer> CreateCustomerAsync(Tenant tenant);
    
    /// <summary>
    /// Update an existing Stripe customer with the tenant's information.
    /// </summary>
    /// <param name="tenant">Tenant for which the customer is to be updated.</param>
    /// <returns>Stripe customer object.</returns>
    Task<StripeCustomer> UpdateCustomerAsync(Tenant tenant);
    
    /// <summary>
    /// Delete a Stripe customer by their ID.
    /// </summary>
    /// <param name="stripeCustomerId">Stripe customer ID.</param>
    Task DeleteCustomerAsync(string stripeCustomerId);
    
    /// <summary>
    /// Create a new subscription in Stripe for the given plan and customer.
    /// </summary>
    /// <param name="plan">Subscription plan.</param>
    /// <param name="tenant">Tenant entity.</param>
    /// <param name="employeeCount">Number of employees in the tenant company to calculate the subscription price.</param>
    /// <returns>Stripe subscription object.</returns>
    Task<StripeSubscription> CreateSubscriptionAsync(SubscriptionPlan plan, Tenant tenant, int employeeCount);
    
    /// <summary>
    /// Cancels a subscription immediately or at period end
    /// </summary>
    /// <param name="stripeSubscriptionId">Stripe subscription ID</param>
    /// <param name="cancelImmediately">Whether to cancel immediately (true) or at period end (false)</param>
    Task CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelImmediately = true);
    
    /// <summary>
    /// Update the quantity of the subscription in Stripe.
    /// </summary>
    /// <param name="stripeSubscriptionId">Stripe subscription ID.</param>
    /// <param name="employeeCount">New number of employees in the tenant company.</param>
    Task UpdateSubscriptionQuantityAsync(string stripeSubscriptionId, int employeeCount);
    
    /// <summary>
    /// Create a new subscription plan in Stripe.
    /// </summary>
    /// <param name="plan">Subscription plan to be created.</param>
    /// <returns>Stripe price and product object.</returns>
    Task<(Product Product, Price Price)> CreateSubscriptionPlanAsync(SubscriptionPlan plan);
    
    /// <summary>
    /// Update an existing subscription plan in Stripe.
    /// </summary>
    /// <param name="plan"> Subscription plan to be updated.</param>
    /// <returns>Stripe price and product object.</returns>
    Task<(Product Product, Price ActivePrice)> UpdateSubscriptionPlanAsync(SubscriptionPlan plan);
    
    /// <summary>
    /// Adds a new payment method to the Stripe customer.
    /// </summary>
    /// <param name="paymentMethod">Payment method entity to add.</param>
    /// <param name="tenant">Tenant entity.</param>
    /// <returns>Stripe PaymentMethod object.</returns>
    Task<StripePaymentMethod> AddPaymentMethodAsync(PaymentMethod paymentMethod, Tenant tenant);

    /// <summary>
    /// Updates an existing payment method in Stripe.
    /// </summary>
    /// <param name="paymentMethod">Payment method entity to update.</param>
    /// <returns>Updated Stripe PaymentMethod object.</returns>
    Task<StripePaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod);

    /// <summary>
    /// Removes a payment method from Stripe.
    /// </summary>
    /// <param name="paymentMethod">Payment method entity to remove.</param>
    Task RemovePaymentMethodAsync(PaymentMethod paymentMethod);

    /// <summary>
    /// Sets the default payment method for a customer.
    /// </summary>
    /// <param name="paymentMethod">Payment method entity to set as default.</param>
    /// <param name="tenant">Tenant entity.</param>
    Task SetDefaultPaymentMethodAsync(PaymentMethod paymentMethod, Tenant tenant);
}