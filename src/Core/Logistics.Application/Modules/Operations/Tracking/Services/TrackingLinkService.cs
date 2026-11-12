using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Operations.Tracking.Services;

internal sealed class TrackingLinkService(
    ITenantUnitOfWork tenantUow,
    IOptions<CustomerPortalOptions> portalOptions) : ITrackingLinkService
{
    private const int ExpirationDays = 30;

    /// <summary>Replaced this close to expiry, so an emailed link lasts the rest of the shipment.</summary>
    private static readonly TimeSpan MinRemainingLifetime = TimeSpan.FromDays(7);

    public async Task<TrackingLink> GetOrCreateAsync(Guid loadId, CancellationToken ct = default)
    {
        var existing = await FindValidAsync(loadId, DateTime.UtcNow + MinRemainingLifetime, ct);

        if (existing is not null)
        {
            return existing;
        }

        var link = new TrackingLink
        {
            Token = TokenGenerator.GenerateSecureToken(64),
            LoadId = loadId,
            ExpiresAt = DateTime.UtcNow.AddDays(ExpirationDays)
        };

        await tenantUow.Repository<TrackingLink>().AddAsync(link, ct);
        return link;
    }

    public Task<TrackingLink?> FindActiveAsync(Guid loadId, CancellationToken ct = default) =>
        FindValidAsync(loadId, DateTime.UtcNow, ct);

    public string GetUrl(TrackingLink link)
    {
        var tenantId = tenantUow.GetCurrentTenant().Id;
        return $"{portalOptions.Value.BaseUrl}/track/{tenantId}/{link.Token}";
    }

    private async Task<TrackingLink?> FindValidAsync(Guid loadId, DateTime validUntil, CancellationToken ct)
    {
        return await tenantUow.Repository<TrackingLink>().Query()
            .Where(l => l.LoadId == loadId && l.IsActive && l.ExpiresAt > validUntil)
            .OrderByDescending(l => l.ExpiresAt)
            .FirstOrDefaultAsync(ct);
    }
}
