using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Logistics.HostDefaults;

public static class RateLimitingExtensions
{
    extension(RateLimiterOptions options)
    {
        /// <summary>Per-IP fixed window with no queue: excess requests are rejected at once.</summary>
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
