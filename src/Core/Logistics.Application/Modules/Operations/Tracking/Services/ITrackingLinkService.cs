using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Tracking.Services;

public interface ITrackingLinkService : IApplicationService
{
    /// <summary>Reuses the newest valid link. The caller owns SaveChangesAsync.</summary>
    Task<TrackingLink> GetOrCreateAsync(Guid loadId, CancellationToken ct = default);

    Task<TrackingLink?> FindActiveAsync(Guid loadId, CancellationToken ct = default);

    string GetUrl(TrackingLink link);

    /// <summary>
    ///     Switches to <paramref name="tenantId" /> and returns the link for <paramref name="token" /> when it is still
    ///     valid - the entry point of every public tracking page, which is reached without a login.
    /// </summary>
    Task<Result<TrackingLink>> OpenPublicLinkAsync(Guid tenantId, string token, CancellationToken ct = default);
}
