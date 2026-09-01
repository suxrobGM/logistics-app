using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Abstractions.Realtime;

/// <summary>
/// Decides whether a caller may subscribe to a trip's realtime group. Lives here so SignalR hubs
/// can enforce it without depending on the Application assembly, the same way
/// <see cref="ITruckGeolocationUpdater"/> carries its own authorization check.
/// </summary>
public interface ITripAccess : IApplicationService
{
    /// <summary>
    /// True when the trip exists in the caller's own tenant. Every trip subscription must go
    /// through this, so the rule lives in one place rather than per adapter.
    /// </summary>
    Task<bool> CanUserViewTripAsync(Guid tenantId, Guid tripId, CancellationToken ct = default);
}
