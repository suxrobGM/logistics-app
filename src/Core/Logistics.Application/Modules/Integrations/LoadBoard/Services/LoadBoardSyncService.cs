using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

internal sealed class LoadBoardSyncService(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderGateway gateway,
    ILogger<LoadBoardSyncService> logger) : ILoadBoardSyncService
{
    private const int ExpiredRetentionDays = 30;

    public async Task ReconcilePostedTrucksAsync(CancellationToken ct)
    {
        var tenantName = tenantUow.GetCurrentTenant().Name;
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
                    providerGroup.Key, tenantName, result.Error);
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

    public async Task CleanupExpiredDataAsync(CancellationToken ct)
    {
        var tenantName = tenantUow.GetCurrentTenant().Name;
        var now = DateTime.UtcNow;

        var expiredListings = await ExpireListingsAsync(now, ct);
        if (expiredListings > 0)
        {
            logger.LogInformation("Marked {Count} expired listings for tenant {TenantName}",
                expiredListings, tenantName);
        }

        var expiredTrucks = await ExpirePostedTrucksAsync(now, ct);
        if (expiredTrucks > 0)
        {
            logger.LogInformation("Marked {Count} expired truck posts for tenant {TenantName}",
                expiredTrucks, tenantName);
        }

        // Saved before the deletes so the expiry writes go through the audit interceptor and stamp
        // UpdatedAt; the deletes below run straight in SQL and only see saved rows.
        await tenantUow.SaveChangesAsync(ct);
        await DeleteOldExpiredDataAsync(now.AddDays(-ExpiredRetentionDays), ct);
    }

    private async Task<int> ExpireListingsAsync(DateTime now, CancellationToken ct)
    {
        var listings = await tenantUow.Repository<LoadBoardListing>()
            .GetListAsync(l => l.ExpiresAt < now && l.Status == LoadBoardListingStatus.Available, ct);

        foreach (var listing in listings)
        {
            listing.Status = LoadBoardListingStatus.Expired;
        }

        return listings.Count;
    }

    private async Task<int> ExpirePostedTrucksAsync(DateTime now, CancellationToken ct)
    {
        var trucks = await tenantUow.Repository<PostedTruck>()
            .GetListAsync(p => p.Status == PostedTruckStatus.Available &&
                               ((p.AvailableTo.HasValue && p.AvailableTo.Value < now) ||
                                (p.ExpiresAt.HasValue && p.ExpiresAt.Value < now)), ct);

        foreach (var truck in trucks)
        {
            truck.Status = PostedTruckStatus.Expired;
        }

        return trucks.Count;
    }

    private async Task DeleteOldExpiredDataAsync(DateTime cutoff, CancellationToken ct)
    {
        await tenantUow.Repository<LoadBoardListing>().Query()
            .Where(l => l.Status == LoadBoardListingStatus.Expired && l.UpdatedAt < cutoff)
            .ExecuteDeleteAsync(ct);

        await tenantUow.Repository<PostedTruck>().Query()
            .Where(p => p.Status == PostedTruckStatus.Expired && p.UpdatedAt < cutoff)
            .ExecuteDeleteAsync(ct);
    }
}
