using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

internal abstract class StripeServiceBase
{
    protected readonly ILogger Logger;

    protected StripeServiceBase(IOptions<StripeOptions> options, ILogger logger)
    {
        Logger = logger;
        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }
}
