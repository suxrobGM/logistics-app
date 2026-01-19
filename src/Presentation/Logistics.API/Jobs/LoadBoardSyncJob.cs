using Hangfire;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that periodically manages load board data for all tenants.
///     - Refreshes posted trucks every 30 minutes to prevent expiration
///     - Cleans up expired listings hourly
/// </summary>
public class LoadBoardSyncJob(
    ILogger<LoadBoardSyncJob> logger,
    IServiceScopeFactory scopeFactory)
{
    /// <summary>
    ///     Schedule the Load Board sync jobs.
    /// </summary>
    public static void ScheduleJobs()
    {
        // Refresh posted trucks every 30 minutes
        RecurringJob.AddOrUpdate<LoadBoardSyncJob>(
            "loadboard-refresh-trucks",
            job => job.RefreshPostedTrucksAsync(CancellationToken.None),
            "*/30 * * * *");

        // Clean up expired listings every hour
        RecurringJob.AddOrUpdate<LoadBoardSyncJob>(
            "loadboard-cleanup",
            job => job.CleanupExpiredDataAsync(CancellationToken.None),
            "0 * * * *");
    }

    /// <summary>
    ///     Refresh all posted trucks to prevent expiration on load boards.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task RefreshPostedTrucksAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();

        var tenants = await masterUow.Repository<Tenant>().GetListAsync(t => t.ConnectionString != null);

        logger.LogInformation("Starting load board truck refresh for {TenantCount} tenants", tenants.Count);

        foreach (var tenant in tenants)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await RefreshTenantTrucksAsync(tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing trucks for tenant {TenantName}", tenant.Name);
            }
        }

        logger.LogInformation("Completed load board truck refresh cycle");
    }

    /// <summary>
    ///     Clean up expired listings and expired truck posts.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task CleanupExpiredDataAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();

        var tenants = await masterUow.Repository<Tenant>().GetListAsync(t => t.ConnectionString != null);

        logger.LogInformation("Starting load board cleanup for {TenantCount} tenants", tenants.Count);

        foreach (var tenant in tenants)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await CleanupTenantDataAsync(tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error cleaning up data for tenant {TenantName}", tenant.Name);
            }
        }

        logger.LogInformation("Completed load board cleanup cycle");
    }

    private async Task RefreshTenantTrucksAsync(Tenant tenant, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        var providerFactory = scope.ServiceProvider.GetRequiredService<ILoadBoardProviderFactory>();

        tenantUow.SetCurrentTenant(tenant);

        // Get all active posted trucks
        var postedTrucks = await tenantUow.Repository<PostedTruck>()
            .GetListAsync(p => p.Status == PostedTruckStatus.Available, ct);

        if (postedTrucks.Count == 0)
        {
            logger.LogDebug("No active posted trucks for tenant {TenantName}", tenant.Name);
            return;
        }

        // Group by provider
        var trucksByProvider = postedTrucks.GroupBy(p => p.ProviderType);

        foreach (var providerGroup in trucksByProvider)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var config = await tenantUow.Repository<LoadBoardConfiguration>()
                .GetAsync(c => c.ProviderType == providerGroup.Key && c.IsActive, ct);

            if (config is null)
            {
                logger.LogWarning("No active config for {ProviderType}, skipping refresh", providerGroup.Key);
                continue;
            }

            var provider = providerFactory.GetProvider(config);

            foreach (var postedTruck in providerGroup)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    if (string.IsNullOrEmpty(postedTruck.ExternalPostId))
                    {
                        continue;
                    }

                    // Refresh the post by updating with same data
                    var updated = await provider.UpdateTruckPostAsync(
                        postedTruck.ExternalPostId,
                        new PostTruckRequest
                        {
                            TruckId = postedTruck.TruckId,
                            ProviderType = postedTruck.ProviderType,
                            AvailableAtAddress = postedTruck.AvailableAtAddress,
                            AvailableAtLocation = postedTruck.AvailableAtLocation,
                            DestinationPreference = postedTruck.DestinationPreference,
                            DestinationRadius = postedTruck.DestinationRadius,
                            AvailableFrom = postedTruck.AvailableFrom,
                            AvailableTo = postedTruck.AvailableTo,
                            EquipmentType = postedTruck.EquipmentType,
                            MaxWeight = postedTruck.MaxWeight,
                            MaxLength = postedTruck.MaxLength
                        });

                    if (updated)
                    {
                        postedTruck.LastRefreshedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        logger.LogWarning(
                            "Failed to refresh posted truck {PostedTruckId} on {ProviderType}",
                            postedTruck.Id, postedTruck.ProviderType);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Error refreshing posted truck {PostedTruckId} on {ProviderType}",
                        postedTruck.Id, postedTruck.ProviderType);
                }
            }
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    private async Task CleanupTenantDataAsync(Tenant tenant, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();

        tenantUow.SetCurrentTenant(tenant);
        var now = DateTime.UtcNow;

        // Clean up expired listings (delete those that expired more than 24 hours ago)
        var expiredListings = await tenantUow.Repository<LoadBoardListing>()
            .GetListAsync(l => l.ExpiresAt < now.AddHours(-24)
                && l.Status == LoadBoardListingStatus.Available, ct);

        foreach (var listing in expiredListings)
        {
            listing.Status = LoadBoardListingStatus.Expired;
        }

        if (expiredListings.Count > 0)
        {
            logger.LogInformation(
                "Marked {Count} expired listings for tenant {TenantName}",
                expiredListings.Count, tenant.Name);
        }

        // Mark posted trucks as expired if past their availability window
        var expiredTrucks = await tenantUow.Repository<PostedTruck>()
            .GetListAsync(p => p.Status == PostedTruckStatus.Available
                && p.AvailableTo.HasValue && p.AvailableTo.Value < now, ct);

        foreach (var truck in expiredTrucks)
        {
            truck.Status = PostedTruckStatus.Expired;
        }

        if (expiredTrucks.Count > 0)
        {
            logger.LogInformation(
                "Marked {Count} expired truck posts for tenant {TenantName}",
                expiredTrucks.Count, tenant.Name);
        }

        // Delete very old expired data (older than 30 days)
        var cutoffDate = now.AddDays(-30);

        var oldListings = await tenantUow.Repository<LoadBoardListing>()
            .GetListAsync(l => l.Status == LoadBoardListingStatus.Expired
                && l.UpdatedAt < cutoffDate, ct);

        foreach (var listing in oldListings)
        {
            tenantUow.Repository<LoadBoardListing>().Delete(listing);
        }

        var oldTrucks = await tenantUow.Repository<PostedTruck>()
            .GetListAsync(p => p.Status == PostedTruckStatus.Expired
                && p.UpdatedAt < cutoffDate, ct);

        foreach (var truck in oldTrucks)
        {
            tenantUow.Repository<PostedTruck>().Delete(truck);
        }

        await tenantUow.SaveChangesAsync(ct);
    }
}
