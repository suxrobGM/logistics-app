using Hangfire;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
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
        // The old job re-sent every post every 30 minutes; its method no longer exists.
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
            scopeFactory, logger, "load board post reconciliation", ReconcileTenantTrucksAsync, ct);

    [AutomaticRetry(Attempts = 2)]
    public Task CleanupExpiredDataAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(
            scopeFactory, logger, "load board cleanup", CleanupTenantDataAsync, ct);

    /// <summary>
    /// A post the board no longer lists expired or was taken down there (by DAT's own expiry,
    /// or a dispatcher in DAT One); mark it expired so nobody plans around it.
    /// </summary>
    private async Task ReconcileTenantTrucksAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        if (!await IsLoadBoardEnabledAsync(scope, tenant))
        {
            return;
        }

        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        var gateway = scope.ServiceProvider.GetRequiredService<ILoadBoardProviderGateway>();
        tenantUow.SetCurrentTenant(tenant);

        var postedTrucks = await tenantUow.Repository<PostedTruck>()
            .GetListAsync(p => p.Status == PostedTruckStatus.Available && p.ExternalPostId != null, ct);

        foreach (var providerGroup in postedTrucks.GroupBy(p => p.ProviderType))
        {
            var config = await tenantUow.Repository<LoadBoardConfiguration>()
                .GetAsync(c => c.ProviderType == providerGroup.Key && c.IsActive, ct);
            if (config is null)
            {
                continue;
            }

            var result = await gateway.ExecuteAsync(config, p => p.GetPostedTrucksAsync(ct), ct);
            if (!result.IsSuccess)
            {
                logger.LogWarning("Could not list {Provider} truck posts for tenant {TenantName}: {Error}",
                    providerGroup.Key, tenant.Name, result.Error);
                continue;
            }

            if (result.Value is not { } boardPosts)
            {
                continue;
            }

            var onBoard = boardPosts.ToDictionary(p => p.ExternalPostId);
            var now = DateTime.UtcNow;
            foreach (var postedTruck in providerGroup)
            {
                if (onBoard.TryGetValue(postedTruck.ExternalPostId!, out var boardPost))
                {
                    postedTruck.ExpiresAt = boardPost.ExpiresAt ?? postedTruck.ExpiresAt;
                    postedTruck.LastRefreshedAt = now;
                }
                else
                {
                    postedTruck.Status = PostedTruckStatus.Expired;
                    logger.LogInformation("{Provider} no longer lists truck post {ExternalPostId}; marked expired",
                        providerGroup.Key, postedTruck.ExternalPostId);
                }
            }

            config.LastSyncedAt = now;
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    private async Task CleanupTenantDataAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        if (!await IsLoadBoardEnabledAsync(scope, tenant))
        {
            return;
        }

        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();

        tenantUow.SetCurrentTenant(tenant);
        var now = DateTime.UtcNow;

        var expiredListings = await tenantUow.Repository<LoadBoardListing>()
            .GetListAsync(l => l.ExpiresAt < now && l.Status == LoadBoardListingStatus.Available, ct);

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

        var expiredTrucks = await tenantUow.Repository<PostedTruck>()
            .GetListAsync(p => p.Status == PostedTruckStatus.Available &&
                               ((p.AvailableTo.HasValue && p.AvailableTo.Value < now) ||
                                (p.ExpiresAt.HasValue && p.ExpiresAt.Value < now)), ct);

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

    /// <summary>
    /// Jobs bypass the request pipeline, so [RequiresFeature] does not gate them.
    /// </summary>
    private static async Task<bool> IsLoadBoardEnabledAsync(IServiceScope scope, Tenant tenant)
    {
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        return await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.LoadBoard);
    }
}
