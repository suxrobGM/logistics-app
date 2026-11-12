using Logistics.Domain.Entities;

namespace Logistics.Application.Abstractions.Payments.Stripe;

public interface IStripePaymentService
{
    /// <summary>
    ///     Creates a new SetupIntent for the given tenant.
    /// </summary>
    /// <returns>The SetupIntent's client secret, which the browser confirms the card with.</returns>
    Task<string> CreateSetupIntentAsync(Tenant tenant);

    /// <summary>
    ///     Creates and confirms a PaymentIntent for the payment.
    /// </summary>
    /// <exception cref="StripeApiException">Stripe rejected the payment.</exception>
    Task<StripePaymentIntentInfo> CreatePaymentIntentAsync(Payment payment, Tenant tenant);
}
