using Logistics.Domain.Entities;

using Stripe;

using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;
using StripeCustomer = Stripe.Customer;
using StripePaymentMethod = Stripe.PaymentMethod;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

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
    /// <param name="trial">Whether to create a trial subscription (true) or not (false). Default is false.</param>
    /// <returns>Stripe subscription object.</returns>
    Task<StripeSubscription> CreateSubscriptionAsync(SubscriptionPlan plan, Tenant tenant, int employeeCount, bool trial = false);

    /// <summary>
    /// Cancels a subscription immediately or at period end
    /// </summary>
    /// <param name="stripeSubscriptionId">Stripe subscription ID</param>
    /// <param name="cancelImmediately">Whether to cancel immediately (true) or at period end (false)</param>
    /// <returns>Stripe subscription item object.</returns>
    Task<StripeSubscription> CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelImmediately = true);

    /// <summary>
    /// Update the quantity of the subscription in Stripe.
    /// </summary>
    /// <param name="stripeSubscriptionId">Stripe subscription ID.</param>
    /// <param name="employeeCount">New number of employees in the tenant company.</param>
    /// <returns>Stripe subscription item object.</returns>
    Task<SubscriptionItem> UpdateSubscriptionQuantityAsync(string stripeSubscriptionId, int employeeCount);

    /// <summary>
    /// Renew an existing subscription or create a new one if it doesn't exist in Stripe.
    /// </summary>
    /// <param name="subEntity">Subscription entity to be renewed.</param>
    /// <param name="plan">Subscription plan to be used for renewal.</param>
    /// <param name="tenant">Tenant entity.</param>
    /// <param name="employeeCount">Number of employees in the tenant company to calculate the subscription price.</param>
    /// <returns>Stripe subscription object.</returns>
    Task<StripeSubscription> RenewSubscriptionAsync(
        Subscription? subEntity,
        SubscriptionPlan plan,
        Tenant tenant,
        int employeeCount);

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

    /// <summary>
    /// Creates a new SetupIntent for the given tenant.
    /// SetupIntents are used to set up future payments without an immediate charge.
    /// For example, this can be used to save a payment method for future use.
    /// </summary>
    /// <param name="tenant">Tenant for which the SetupIntent is to be created.</param>
    /// <returns>Stripe SetupIntent object.</returns>
    Task<SetupIntent> CreateSetupIntentAsync(Tenant tenant);

    /// <summary>
    /// Creates a PaymentIntent for processing a payment.
    /// </summary>
    /// <param name="payment">Payment entity with amount details.</param>
    /// <param name="paymentMethod">The payment method to use.</param>
    /// <param name="tenant">Tenant entity for customer reference.</param>
    /// <returns>Stripe PaymentIntent object.</returns>
    Task<PaymentIntent> CreatePaymentIntentAsync(Payment payment, PaymentMethod paymentMethod, Tenant tenant);

    /// <summary>
    /// Retrieves a PaymentIntent from Stripe.
    /// </summary>
    /// <param name="paymentIntentId">The Stripe PaymentIntent ID.</param>
    /// <returns>Stripe PaymentIntent object.</returns>
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);

    /// <summary>
    /// Attaches an existing Stripe payment method to a customer.
    /// </summary>
    /// <param name="stripePaymentMethodId">The Stripe payment method ID.</param>
    /// <param name="tenant">Tenant entity for customer reference.</param>
    /// <returns>Attached Stripe PaymentMethod object.</returns>
    Task<StripePaymentMethod> AttachPaymentMethodAsync(string stripePaymentMethodId, Tenant tenant);

    /// <summary>
    /// Retrieves a payment method from Stripe to verify it exists.
    /// </summary>
    /// <param name="stripePaymentMethodId">The Stripe payment method ID.</param>
    /// <returns>Stripe PaymentMethod object if found, null otherwise.</returns>
    Task<StripePaymentMethod?> GetPaymentMethodAsync(string stripePaymentMethodId);
}
