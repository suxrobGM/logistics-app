using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Operations.Tracking.Services;

/// <summary>
/// Owns public tracking links: reuse of the active link and the URL format.
/// </summary>
public interface ITrackingLinkService : IApplicationService
{
    /// <summary>
    /// Returns the load's newest valid link, or adds a new one. The caller owns SaveChangesAsync.
    /// </summary>
    Task<TrackingLink> GetOrCreateAsync(Guid loadId, CancellationToken ct = default);

    /// <summary>
    /// The customer-portal URL that opens the link for the current tenant.
    /// </summary>
    string GetUrl(TrackingLink link);
}
