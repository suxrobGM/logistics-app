using Hangfire;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
///     Pulls ELD data for every tenant through <see cref="IEldSyncService" />: HOS clocks and vehicle GPS
///     every 5 minutes, HOS logs and violations every 30 minutes.
/// </summary>
public class EldSyncJob(
    ILogger<EldSyncJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<EldSyncJob>(
            "eld-sync",
            job => job.SyncAllTenantsAsync(CancellationToken.None),
            Cron.MinuteInterval(5));

        RecurringJob.AddOrUpdate<EldSyncJob>(
            "eld-history-sync",
            job => job.SyncHistoryAllTenantsAsync(CancellationToken.None),
            Cron.MinuteInterval(30));
    }

    [AutomaticRetry(Attempts = 2)]
    public Task SyncAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "ELD sync",
            (scope, tenant, token) => ForEachConfigAsync(scope, tenant,
                (sync, config, c) => sync.SyncLiveAsync(config, c), token), ct);

    [AutomaticRetry(Attempts = 2)]
    public Task SyncHistoryAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "ELD history sync",
            (scope, tenant, token) => ForEachConfigAsync(scope, tenant,
                (sync, config, c) => sync.SyncHistoryAsync(config, c), token), ct);

    private async Task ForEachConfigAsync(
        IServiceScope scope,
        Tenant tenant,
        Func<IEldSyncService, EldProviderConfiguration, CancellationToken, Task> body,
        CancellationToken ct)
    {
        // Jobs bypass the request pipeline, so [RequiresFeature] does not gate them - check here
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.Eld))
        {
            return;
        }

        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        tenantUow.SetCurrentTenant(tenant);

        var configurations = await tenantUow.Repository<EldProviderConfiguration>()
            .GetListAsync(c => c.IsActive, ct);
        var sync = scope.ServiceProvider.GetRequiredService<IEldSyncService>();

        foreach (var config in configurations)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await body(sync, config, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing {ProviderType} data for tenant {TenantName}",
                    config.ProviderType, tenant.Name);
            }
        }
    }
}
