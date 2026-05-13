using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripeService(IOptions<StripeOptions> options) : IStripeService
{
    public string WebhookSecret { get; } = options.Value.WebhookSecret ??
                                           throw new ArgumentNullException(nameof(options.Value.WebhookSecret));
}
