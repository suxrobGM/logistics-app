using System.Globalization;
using System.Net;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// Retries a request the vendor answered with <c>429 Too Many Requests</c>, waiting for the
/// <c>Retry-After</c> the vendor asked for (seconds, fractions allowed - Samsara sends e.g. <c>0.40235</c>),
/// or a short exponential back-off when the header is absent. Gives up after <see cref="MaxRetries"/>
/// and hands the last 429 to the caller. Attach with <c>AddHttpMessageHandler</c>.
/// </summary>
public sealed class RateLimitRetryHandler : DelegatingHandler
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(30);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        for (var attempt = 0; ; attempt++)
        {
            var response = await base.SendAsync(request, ct);
            if (response.StatusCode != HttpStatusCode.TooManyRequests || attempt == MaxRetries)
            {
                return response;
            }

            var delay = GetRetryDelay(response, attempt);
            response.Dispose();
            await Task.Delay(delay, ct);
        }
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        // Parsed by hand: the typed RetryAfter header rejects fractional seconds.
        if (response.Headers.NonValidated.TryGetValues("Retry-After", out var values))
        {
            var raw = values.FirstOrDefault();
            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) && seconds >= 0)
            {
                return TimeSpan.FromSeconds(Math.Min(seconds, MaxDelay.TotalSeconds));
            }

            if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var at))
            {
                var wait = at - DateTimeOffset.UtcNow;
                return wait <= TimeSpan.Zero ? TimeSpan.Zero : wait < MaxDelay ? wait : MaxDelay;
            }
        }

        return TimeSpan.FromSeconds(Math.Pow(2, attempt));
    }
}
