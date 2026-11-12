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

    /// <summary>
    /// A link this close to expiry is replaced, so an emailed link stays usable for the rest of the shipment.
    /// </summary>
    private static readonly TimeSpan MinRemainingLifetime = TimeSpan.FromDays(7);

    public async Task<TrackingLink> GetOrCreateAsync(Guid loadId, CancellationToken ct = default)
    {
        var usableUntil = DateTime.UtcNow + MinRemainingLifetime;
        var existing = await tenantUow.Repository<TrackingLink>().Query()
            .Where(l => l.LoadId == loadId && l.IsActive && l.ExpiresAt > usableUntil)
            .OrderByDescending(l => l.ExpiresAt)
            .FirstOrDefaultAsync(ct);

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

    public async Task<TrackingLink?> FindActiveAsync(Guid loadId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await tenantUow.Repository<TrackingLink>().Query()
            .Where(l => l.LoadId == loadId && l.IsActive && l.ExpiresAt > now)
            .OrderByDescending(l => l.ExpiresAt)
            .FirstOrDefaultAsync(ct);
    }

    public string GetUrl(TrackingLink link)
    {
        var tenantId = tenantUow.GetCurrentTenant().Id;
        return $"{portalOptions.Value.BaseUrl}/track/{tenantId}/{link.Token}";
    }
}
