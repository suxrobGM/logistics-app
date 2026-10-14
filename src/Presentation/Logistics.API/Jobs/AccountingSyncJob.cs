using Hangfire;
using Logistics.Application.Modules.Integrations.Accounting.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.API.Jobs;

/// <summary>
///     Pushes every tenant's financial data to its connected accounting provider (QuickBooks Online)
///     through <see cref="IAccountingSyncService"/> every 15 minutes. The service checks the
///     Accounting feature itself, since jobs bypass <c>[RequiresFeature]</c>.
/// </summary>
public class AccountingSyncJob(
    ILogger<AccountingSyncJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<AccountingSyncJob>(
            "accounting-sync",
            job => job.SyncAllTenantsAsync(CancellationToken.None),
            Cron.MinuteInterval(15));
    }

    /// <remarks>A first sync of a large history can outlast the interval; never start a second cycle on top.</remarks>
    [AutomaticRetry(Attempts = 2)]
    [DisableConcurrentExecution(timeoutInSeconds: 30 * 60)]
    public Task SyncAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "accounting sync", SyncTenantAsync, ct);

    private static Task SyncTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>().SetCurrentTenant(tenant);
        return scope.ServiceProvider.GetRequiredService<IAccountingSyncService>().SyncAsync(ct);
    }
}
