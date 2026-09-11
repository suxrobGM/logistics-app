using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Abstractions.Realtime;

/// <summary>
/// Decides whether a caller may subscribe to a trip's realtime group. Lives here so SignalR hubs
/// can enforce it without depending on the Application assembly, like <see cref="ITruckGeolocationUpdater"/>.
/// </summary>
public interface ITripAccess : IApplicationService
{
    /// <summary>
    /// True when the trip exists in the caller's own tenant and the caller is either dispatch or a
    /// driver of the trip's assigned truck.
    /// </summary>
    Task<bool> CanUserViewTripAsync(
        Guid tenantId, Guid tripId, Guid userId, CancellationToken ct = default);
}
