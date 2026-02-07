using Logistics.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Payments.Stripe;

internal class StripeService(IOptions<StripeOptions> options, ILogger<StripeService> logger)
    : StripeServiceBase(options, logger), IStripeService
{
    public string WebhookSecret { get; } = options.Value.WebhookSecret ??
                                           throw new ArgumentNullException(nameof(options.Value.WebhookSecret));
}
