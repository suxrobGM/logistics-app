using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Routing;

namespace Logistics.Application.Abstractions.Routing;

/// <summary>
/// Service for broadcasting trip-related updates to connected clients.
/// </summary>
public interface ITripTrackingService
{
    /// <summary>
    /// Broadcasts a trip status change to subscribers of the specific trip and the tenant.
    /// </summary>
    Task BroadcastTripStatusUpdateAsync(Guid tenantId, TripStatusUpdateDto update);

    /// <summary>
    /// Broadcasts a stop arrival to subscribers of the specific trip and the tenant.
    /// </summary>
    Task BroadcastStopArrivalAsync(Guid tenantId, StopArrivalUpdateDto update);

    /// <summary>
    /// Broadcasts a dispatch board update to dispatch board subscribers.
    /// </summary>
    Task BroadcastDispatchBoardUpdateAsync(Guid tenantId, DispatchBoardUpdateDto update);

}
