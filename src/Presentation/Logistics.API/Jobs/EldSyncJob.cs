using Hangfire;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that periodically syncs HOS data from ELD providers for all tenants.
///     Runs every 5 minutes as a fallback for webhook delivery failures.
/// </summary>
public class EldSyncJob(
    ILogger<EldSyncJob> logger,
    IServiceScopeFactory scopeFactory,
    IHubContext<TrackingHub, ITrackingHubClient> trackingHub)
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
                await SyncProviderDataAsync(tenantUow, eldFactory, config, tenant, ct);
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
        Tenant tenant,
        CancellationToken ct)
    {
        var provider = eldFactory.GetProvider(config);

        // HOS sync (skip for GPS-only providers that return no HOS data)
        var driverMappings = await tenantUow.Repository<EldDriverMapping>()
            .GetListAsync(m => m.ProviderType == config.ProviderType && m.IsSyncEnabled, ct);

        if (driverMappings.Count > 0)
        {
            logger.LogDebug("Syncing HOS data for {Count} drivers from {ProviderType}",
                driverMappings.Count, config.ProviderType);

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

                await UpdateDriverHosStatusAsync(tenantUow, mapping, hosData, config.ProviderType, ct);
            }
        }

        // GPS tracking sync (for providers that support it, e.g., TT ELD)
        if (provider is IEldGpsTrackingProvider gpsProvider)
        {
            await SyncVehicleLocationsAsync(tenantUow, gpsProvider, config, tenant, ct);
        }

        // Update configuration last synced time
        config.LastSyncedAt = DateTime.UtcNow;
        await tenantUow.SaveChangesAsync(ct);

        logger.LogDebug("Completed ELD sync for {ProviderType}", config.ProviderType);
    }

    private async Task SyncVehicleLocationsAsync(
        ITenantUnitOfWork tenantUow,
        IEldGpsTrackingProvider gpsProvider,
        EldProviderConfiguration config,
        Tenant tenant,
        CancellationToken ct)
    {
        var locations = (await gpsProvider.GetAllVehicleLocationsAsync(ct)).ToList();

        if (locations.Count == 0)
        {
            logger.LogDebug("No vehicle locations from {ProviderType}", config.ProviderType);
            return;
        }

        var vehicleMappings = await tenantUow.Repository<EldVehicleMapping>()
            .GetListAsync(m => m.ProviderType == config.ProviderType && m.IsSyncEnabled, ct);

        if (vehicleMappings.Count == 0)
        {
            return;
        }

        var locationMap = locations
            .GroupBy(l => l.ExternalVehicleId)
            .ToDictionary(g => g.Key, g => g.First());

        var syncedCount = 0;
        foreach (var mapping in vehicleMappings)
        {
            if (!locationMap.TryGetValue(mapping.ExternalVehicleId, out var location))
            {
                continue;
            }

            var truck = mapping.Truck;
            truck.CurrentLocation = new GeoPoint(location.Latitude, location.Longitude);
            mapping.LastSyncedAt = DateTime.UtcNow;
            syncedCount++;

            var geolocationDto = new TruckGeolocationDto
            {
                TruckId = truck.Id,
                TenantId = tenant.Id,
                CurrentLocation = new GeoPoint(location.Latitude, location.Longitude),
                TruckNumber = truck.Number
            };

            await trackingHub.Clients
                .Group(tenant.Id.ToString())
                .ReceiveGeolocationData(geolocationDto);
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogDebug("Synced {Count} vehicle locations from {ProviderType}",
            syncedCount, config.ProviderType);
    }

    private static async Task UpdateDriverHosStatusAsync(
        ITenantUnitOfWork tenantUow,
        EldDriverMapping mapping,
        EldDriverHosDataDto hosData,
        EldProviderType providerType,
        CancellationToken ct)
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

        await tenantUow.SaveChangesAsync(ct);
    }
}
