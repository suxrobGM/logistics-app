using Hangfire;
using Logistics.Application.Modules.Compliance.Privacy.Services;

namespace Logistics.API.Jobs;

/// <summary>Purges each tenant's data past its GDPR retention window.</summary>
public class DataRetentionJob(
    ILogger<DataRetentionJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<DataRetentionJob>(
            "privacy-retention",
            job => job.PurgeAllTenantsAsync(CancellationToken.None),
            Cron.Daily(4));
    }

    public Task PurgeAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "retention purge",
            (scope, tenant, token) => scope.ServiceProvider.GetRequiredService<IDataRetentionService>().PurgeAsync(tenant, token),
            ct);
}
