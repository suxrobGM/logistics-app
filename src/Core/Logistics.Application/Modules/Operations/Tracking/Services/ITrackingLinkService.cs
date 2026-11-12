using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Tracking.Services;

public interface ITrackingLinkService : IApplicationService
{
    /// <summary>Reuses the newest valid link. The caller owns SaveChangesAsync.</summary>
    Task<TrackingLink> GetOrCreateAsync(Guid loadId, CancellationToken ct = default);

    Task<TrackingLink?> FindActiveAsync(Guid loadId, CancellationToken ct = default);

    string GetUrl(TrackingLink link);

    /// <summary>Sets the current tenant to <paramref name="tenantId" /> first, because a public tracking page has no signed-in user to take it from.</summary>
    Task<Result<TrackingLink>> OpenPublicLinkAsync(Guid tenantId, string token, CancellationToken ct = default);
}
