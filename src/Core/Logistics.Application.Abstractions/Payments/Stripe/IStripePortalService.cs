using Logistics.Application.Abstractions.Payments.Stripe;
namespace Logistics.Application.Abstractions.Payments.Stripe;

public interface IStripePortalService
{
    /// <summary>
    ///     Creates a Stripe Customer Portal session for managing billing, payment methods, and invoices.
    /// </summary>
    Task<string> CreateBillingPortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct = default);
}
