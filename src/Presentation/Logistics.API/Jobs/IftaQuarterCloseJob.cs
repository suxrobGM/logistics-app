using Hangfire;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
///     Daily job that snapshots the previous IFTA quarter for tenants with the IFTA feature
///     (idempotent — a quarter is snapshotted once, then served immutably) and purges GPS
///     breadcrumbs past the 4-year audit window.
/// </summary>
public class IftaQuarterCloseJob(
    ILogger<IftaQuarterCloseJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<IftaQuarterCloseJob>(
            "ifta-quarter-close",
            job => job.ProcessAllTenantsAsync(CancellationToken.None),
            Cron.Daily(3));
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task ProcessAllTenantsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();
        var tenants = await masterUow.Repository<Tenant>().GetListAsync(t => t.ConnectionString != null);

        foreach (var tenant in tenants)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await ProcessTenantAsync(tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error closing IFTA quarter for tenant {TenantName}", tenant.Name);
            }
        }
    }

    private async Task ProcessTenantAsync(Tenant tenant, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();

        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        tenantUow.SetCurrentTenant(tenant);

        var closeService = scope.ServiceProvider.GetRequiredService<IIftaQuarterCloseService>();

        if (await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.Ifta))
        {
            await closeService.CloseQuarterForCurrentTenantAsync(ct);
        }

        // Purge regardless of the IFTA flag: TruckLocationRecorder writes a breadcrumb on every
        // ELD GPS ping, which is not IFTA-gated. Skipping the purge for non-IFTA tenants would
        // grow the table forever and retain GPS history indefinitely.
        var purged = await closeService.PurgeExpiredLocationHistoryAsync(ct);
        if (purged > 0)
        {
            logger.LogInformation("Purged {Count} expired location breadcrumbs for tenant {TenantName}",
                purged, tenant.Name);
        }
    }
}
