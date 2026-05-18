using Logistics.Application.Abstractions.Payments.Stripe;
namespace Logistics.Application.Abstractions.Payments.Stripe;

public interface IStripeService
{
    /// <summary>
    ///     Stripe webhook secret
    /// </summary>
    string WebhookSecret { get; }
}
