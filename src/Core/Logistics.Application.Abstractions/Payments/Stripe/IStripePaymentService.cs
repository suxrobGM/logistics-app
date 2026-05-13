using Logistics.Domain.Entities;
using Stripe;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Application.Abstractions.Payments.Stripe;

public interface IStripePaymentService
{
    /// <summary>
    ///     Creates a new SetupIntent for the given tenant.
    /// </summary>
    Task<SetupIntent> CreateSetupIntentAsync(Tenant tenant);

    /// <summary>
    ///     Creates a PaymentIntent for processing a payment.
    /// </summary>
    Task<PaymentIntent> CreatePaymentIntentAsync(Payment payment, Tenant tenant);

    /// <summary>
    ///     Retrieves a PaymentIntent from Stripe.
    /// </summary>
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);
}
