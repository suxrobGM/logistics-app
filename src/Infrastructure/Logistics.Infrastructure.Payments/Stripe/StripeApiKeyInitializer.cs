using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
/// Sets <see cref="StripeConfiguration.ApiKey"/> once at host startup. Stripe.NET keeps the key
/// in static state, so every Stripe-using service in this codebase relies on it having been set
/// before the first SDK call. Registering this as an <see cref="IHostedService"/> guarantees that
/// without making each service repeat the bootstrap.
/// </summary>
internal sealed class StripeApiKeyInitializer(
    IOptions<StripeOptions> options,
    ILogger<StripeApiKeyInitializer> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var key = options.Value.SecretKey;
        if (string.IsNullOrEmpty(key))
        {
            logger.LogWarning("Stripe SecretKey is not configured; SDK calls will fail.");
            return Task.CompletedTask;
        }

        StripeConfiguration.ApiKey = key;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
