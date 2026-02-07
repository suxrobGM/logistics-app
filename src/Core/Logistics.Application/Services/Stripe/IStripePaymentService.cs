using Logistics.Domain.Entities;
using Stripe;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;
using StripePaymentMethod = Stripe.PaymentMethod;

namespace Logistics.Application.Services;

public interface IStripePaymentService
{
    /// <summary>
    ///     Creates a new SetupIntent for the given tenant.
    /// </summary>
    Task<SetupIntent> CreateSetupIntentAsync(Tenant tenant);

    /// <summary>
    ///     Creates a PaymentIntent for processing a payment.
    /// </summary>
    Task<PaymentIntent> CreatePaymentIntentAsync(Payment payment, PaymentMethod paymentMethod, Tenant tenant);

    /// <summary>
    ///     Retrieves a PaymentIntent from Stripe.
    /// </summary>
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);

    /// <summary>
    ///     Updates an existing payment method in Stripe.
    /// </summary>
    Task<StripePaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod);

    /// <summary>
    ///     Removes a payment method from Stripe.
    /// </summary>
    Task RemovePaymentMethodAsync(PaymentMethod paymentMethod);

    /// <summary>
    ///     Sets the default payment method for a customer.
    /// </summary>
    Task SetDefaultPaymentMethodAsync(PaymentMethod paymentMethod, Tenant tenant);

    /// <summary>
    ///     Attaches an existing Stripe payment method to a customer.
    /// </summary>
    Task<StripePaymentMethod> AttachPaymentMethodAsync(string stripePaymentMethodId, Tenant tenant);

    /// <summary>
    ///     Retrieves a payment method from Stripe to verify it exists.
    /// </summary>
    Task<StripePaymentMethod?> GetPaymentMethodAsync(string stripePaymentMethodId);
}
