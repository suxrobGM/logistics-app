using Hangfire;
using Logistics.API.Jobs.Scheduling;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
///     Keeps load board data honest for every tenant:
///     - every 30 minutes, reconciles active truck posts with what each board still holds
///     - hourly, expires stale listings and posts past their window, and deletes old ones
/// </summary>
public class LoadBoardSyncJob(
    ILogger<LoadBoardSyncJob> logger,
    IServiceScopeFactory scopeFactory)
{
    private const string LegacyRefreshJobId = "loadboard-refresh-trucks";

    public static void ScheduleJobs()
    {
        // Hangfire still stores the retired job, and its method is gone.
        RecurringJob.RemoveIfExists(LegacyRefreshJobId);

        RecurringJob.AddOrUpdate<LoadBoardSyncJob>(
            "loadboard-reconcile-trucks",
            job => job.ReconcilePostedTrucksAsync(CancellationToken.None),
            Cron.MinuteInterval(30));

        RecurringJob.AddOrUpdate<LoadBoardSyncJob>(
            "loadboard-cleanup",
            job => job.CleanupExpiredDataAsync(CancellationToken.None),
            Cron.Hourly());
    }

    [AutomaticRetry(Attempts = 2)]
    public Task ReconcilePostedTrucksAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(
            scopeFactory, logger, "load board post reconciliation",
            (scope, tenant, token) => RunIfEnabledAsync(scope, tenant, (sync, t) => sync.ReconcilePostedTrucksAsync(t), token),
            ct);

    [AutomaticRetry(Attempts = 2)]
    public Task CleanupExpiredDataAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(
            scopeFactory, logger, "load board cleanup",
            (scope, tenant, token) => RunIfEnabledAsync(scope, tenant, (sync, t) => sync.CleanupExpiredDataAsync(t), token),
            ct);

    private static async Task RunIfEnabledAsync(
        IServiceScope scope,
        Tenant tenant,
        Func<ILoadBoardSyncService, CancellationToken, Task> run,
        CancellationToken ct)
    {
        if (!await IsLoadBoardEnabledAsync(scope, tenant))
        {
            return;
        }

        await run(scope.ServiceProvider.GetRequiredService<ILoadBoardSyncService>(), ct);
    }

    /// <summary>
    /// Jobs bypass the request pipeline, so [RequiresFeature] does not gate them.
    /// </summary>
    private static async Task<bool> IsLoadBoardEnabledAsync(IServiceScope scope, Tenant tenant)
    {
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        return await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.LoadBoard);
    }
}
