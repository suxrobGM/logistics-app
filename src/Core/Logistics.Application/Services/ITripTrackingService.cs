using Logistics.Shared.Models;

namespace Logistics.Application.Services;

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

    /// <summary>
    /// Broadcasts an AI dispatch agent session status update.
    /// </summary>
    Task BroadcastDispatchAgentUpdateAsync(Guid tenantId, DispatchAgentUpdateDto update);

    /// <summary>
    /// Broadcasts an individual AI dispatch agent decision notification.
    /// </summary>
    Task BroadcastDispatchDecisionAsync(Guid tenantId, DispatchDecisionDto decision);
}
