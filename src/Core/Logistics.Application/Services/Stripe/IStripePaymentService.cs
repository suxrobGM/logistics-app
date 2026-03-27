using Logistics.Domain.Entities;
using Stripe;

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
    Task<PaymentIntent> CreatePaymentIntentAsync(Payment payment, Tenant tenant);

    /// <summary>
    ///     Retrieves a PaymentIntent from Stripe.
    /// </summary>
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);
}
