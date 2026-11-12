using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Logistics.HostDefaults;

public static class RateLimitingExtensions
{
    extension(RateLimiterOptions options)
    {
        /// <summary>
        /// Lets each client IP make <paramref name="permitLimit" /> requests per <paramref name="window" />; the rest
        /// fail with 503 straight away. Requests with no IP share one limit.
        /// </summary>
        public RateLimiterOptions AddIpFixedWindowPolicy(
            string policyName, int permitLimit, TimeSpan window)
        {
            options.AddPolicy(policyName, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = permitLimit,
                        Window = window,
                        QueueLimit = 0
                    }));

            return options;
        }
    }
}
