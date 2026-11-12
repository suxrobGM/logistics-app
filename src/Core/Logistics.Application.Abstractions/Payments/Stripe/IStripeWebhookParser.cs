namespace Logistics.Application.Abstractions.Payments.Stripe;

public interface IStripeWebhookParser
{
    /// <summary>
    /// Verifies the <c>Stripe-Signature</c> header against the webhook secret and reads the event.
    /// Returns null when the signature does not verify.
    /// </summary>
    StripeWebhookEvent? Parse(string requestBody, string signature);
}
