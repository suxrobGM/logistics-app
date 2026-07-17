using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Logistics.HostDefaults;

/// <summary>
/// Shared rate-limiting building blocks for the presentation hosts.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds a strict per-IP fixed-window policy (no queue), used by both hosts to throttle the
    /// impersonation endpoints.
    /// </summary>
    public static RateLimiterOptions AddIpFixedWindowPolicy(
        this RateLimiterOptions options, string policyName, int permitLimit, TimeSpan window)
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
