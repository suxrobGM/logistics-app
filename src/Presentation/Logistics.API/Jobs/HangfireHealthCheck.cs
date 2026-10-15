using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Logistics.API.Jobs;

/// <summary>
/// Reports whether Hangfire storage answers and at least one job server has sent a heartbeat
/// recently. Without a live server the recurring syncs (ELD, load board, accounting, payroll)
/// silently stop while the API keeps serving requests.
/// </summary>
internal sealed class HangfireHealthCheck(JobStorage storage, TimeProvider timeProvider) : IHealthCheck
{
    /// <summary>Hangfire's own server timeout is 5 minutes; a heartbeat older than that is a dead server.</summary>
    private static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromMinutes(5);

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var monitoring = storage.GetMonitoringApi();
            var cutoff = timeProvider.GetUtcNow().UtcDateTime - HeartbeatTimeout;
            var liveServers = monitoring.Servers().Count(s => s.Heartbeat >= cutoff);
            var failedJobs = monitoring.FailedCount();
            var data = new Dictionary<string, object> { ["liveServers"] = liveServers, ["failedJobs"] = failedJobs };

            return Task.FromResult(liveServers > 0
                ? HealthCheckResult.Healthy(data: data)
                : HealthCheckResult.Unhealthy("No Hangfire server has sent a heartbeat in the last 5 minutes.", data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire storage is unreachable.", ex));
        }
    }
}
