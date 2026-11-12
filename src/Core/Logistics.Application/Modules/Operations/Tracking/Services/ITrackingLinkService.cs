using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Operations.Tracking.Services;

public interface ITrackingLinkService : IApplicationService
{
    /// <summary>Reuses the newest valid link. The caller owns SaveChangesAsync.</summary>
    Task<TrackingLink> GetOrCreateAsync(Guid loadId, CancellationToken ct = default);

    Task<TrackingLink?> FindActiveAsync(Guid loadId, CancellationToken ct = default);

    string GetUrl(TrackingLink link);
}
