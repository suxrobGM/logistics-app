using Logistics.Application.Services;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripeService(IOptions<StripeOptions> options) : IStripeService
{
    public string WebhookSecret { get; } = options.Value.WebhookSecret ??
                                           throw new ArgumentNullException(nameof(options.Value.WebhookSecret));
}
