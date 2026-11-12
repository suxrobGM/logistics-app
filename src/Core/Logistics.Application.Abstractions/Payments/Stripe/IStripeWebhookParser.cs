namespace Logistics.Application.Abstractions.Payments.Stripe;

public interface IStripeWebhookParser
{
    /// <summary>Returns null when the <c>Stripe-Signature</c> header does not verify.</summary>
    StripeWebhookEvent? Parse(string requestBody, string signature);
}
