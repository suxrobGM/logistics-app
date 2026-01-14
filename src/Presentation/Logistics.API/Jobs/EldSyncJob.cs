using Hangfire;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that periodically syncs HOS data from ELD providers for all tenants.
///     Runs every 5 minutes as a fallback for webhook delivery failures.
/// </summary>
public class EldSyncJob(
    ILogger<EldSyncJob> logger,
    IServiceScopeFactory scopeFactory)
{
    /// <summary>
    ///     Schedule the ELD sync job to run every 5 minutes.
    /// </summary>
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<EldSyncJob>(
            "eld-sync",
            job => job.SyncAllTenantsAsync(CancellationToken.None),
            "*/5 * * * *"); // Every 5 minutes
    }

    /// <summary>
    ///     Main job entry point - syncs HOS data for all tenants.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task SyncAllTenantsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();

        // Get all active tenants
        var tenants = await masterUow.Repository<Tenant>().GetListAsync(t => t.ConnectionString != null);

        logger.LogInformation("Starting ELD sync for {TenantCount} tenants", tenants.Count);

        foreach (var tenant in tenants)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await SyncTenantAsync(tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing ELD data for tenant {TenantName}", tenant.Name);
            }
        }

        logger.LogInformation("Completed ELD sync cycle");
    }

    private async Task SyncTenantAsync(Tenant tenant, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        var eldFactory = scope.ServiceProvider.GetRequiredService<IEldProviderFactory>();

        // Switch to tenant database
        tenantUow.SetCurrentTenant(tenant);

        // Get active ELD configurations for this tenant
        var configurations = await tenantUow.Repository<EldProviderConfiguration>()
            .GetListAsync(c => c.IsActive, ct);

        if (configurations.Count == 0)
        {
            logger.LogDebug("No active ELD configurations for tenant {TenantName}", tenant.Name);
            return;
        }

        foreach (var config in configurations)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await SyncProviderDataAsync(tenantUow, eldFactory, config, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing {ProviderType} data for tenant {TenantName}",
                    config.ProviderType, tenant.Name);
            }
        }
    }

    private async Task SyncProviderDataAsync(
        ITenantUnitOfWork tenantUow,
        IEldProviderFactory eldFactory,
        EldProviderConfiguration config,
        CancellationToken ct)
    {
        var provider = eldFactory.GetProvider(config);

        // Get all driver mappings for this provider
        var driverMappings = await tenantUow.Repository<EldDriverMapping>()
            .GetListAsync(m => m.ProviderType == config.ProviderType && m.IsSyncEnabled, ct);

        if (driverMappings.Count == 0)
        {
            logger.LogDebug("No driver mappings for {ProviderType}", config.ProviderType);
            return;
        }

        logger.LogDebug("Syncing HOS data for {Count} drivers from {ProviderType}",
            driverMappings.Count, config.ProviderType);

        // Fetch all drivers HOS status in one call
        var allHosData = await provider.GetAllDriversHosStatusAsync();
        var hosDataMap = allHosData.ToDictionary(h => h.ExternalDriverId);

        foreach (var mapping in driverMappings)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            if (!hosDataMap.TryGetValue(mapping.ExternalDriverId, out var hosData))
            {
                logger.LogWarning("No HOS data found for external driver {ExternalDriverId}",
                    mapping.ExternalDriverId);
                continue;
            }

            await UpdateDriverHosStatusAsync(tenantUow, mapping, hosData, config.ProviderType);
        }

        // Update configuration last synced time
        config.LastSyncedAt = DateTime.UtcNow;
        await tenantUow.SaveChangesAsync(ct);

        logger.LogDebug("Completed ELD sync for {ProviderType}", config.ProviderType);
    }

    private static async Task UpdateDriverHosStatusAsync(
        ITenantUnitOfWork tenantUow,
        EldDriverMapping mapping,
        EldDriverHosDataDto hosData,
        EldProviderType providerType)
    {
        var existingStatus = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(s => s.EmployeeId == mapping.EmployeeId);

        if (existingStatus == null)
        {
            existingStatus = new DriverHosStatus
            {
                EmployeeId = mapping.EmployeeId,
                ExternalDriverId = mapping.ExternalDriverId,
                ProviderType = providerType,
                CurrentDutyStatus = hosData.CurrentDutyStatus
            };
            await tenantUow.Repository<DriverHosStatus>().AddAsync(existingStatus);
        }
        else
        {
            existingStatus.CurrentDutyStatus = hosData.CurrentDutyStatus;
        }

        existingStatus.StatusChangedAt = hosData.StatusChangedAt;
        existingStatus.DrivingMinutesRemaining = hosData.DrivingMinutesRemaining;
        existingStatus.OnDutyMinutesRemaining = hosData.OnDutyMinutesRemaining;
        existingStatus.CycleMinutesRemaining = hosData.CycleMinutesRemaining;
        existingStatus.TimeUntilBreakRequired = hosData.TimeUntilBreakRequired;
        existingStatus.IsInViolation = hosData.IsInViolation;
        existingStatus.LastUpdatedAt = DateTime.UtcNow;

        // Update mapping last synced time
        mapping.LastSyncedAt = DateTime.UtcNow;

        await tenantUow.SaveChangesAsync();
    }
}
