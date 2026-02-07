namespace Logistics.Application.Services;

public interface IStripeService
{
    /// <summary>
    ///     Stripe webhook secret
    /// </summary>
    string WebhookSecret { get; }
}
