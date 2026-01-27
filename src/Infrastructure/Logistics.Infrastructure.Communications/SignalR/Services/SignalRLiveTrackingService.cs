using Logistics.Application.Services.Realtime;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

/// <summary>
///     SignalR implementation of a real-time live tracking service (GPS, trip updates).
/// </summary>
internal sealed class SignalRLiveTrackingService(IHubContext<TrackingHub, ITrackingHubClient> hubContext)
    : IRealtimeLiveTrackingService
{
    public async Task BroadcastGeolocationDataAsync(
        string tenantId,
        TruckGeolocationDto geolocationData,
        CancellationToken ct = default)
    {
        await hubContext.Clients
            .Group(tenantId)
            .ReceiveGeolocationData(geolocationData);
    }

    public async Task BroadcastTripStatusUpdateAsync(
        string tenantId,
        Guid tripId,
        TripStatus newStatus,
        CancellationToken ct = default)
    {
        var statusUpdate = new TripStatusUpdateDto { TripId = tripId, Status = newStatus, UpdatedAt = DateTime.UtcNow };

        await hubContext.Clients
            .Groups(tenantId, $"trip:{tripId}")
            .ReceiveTripStatusUpdate(statusUpdate);
    }

    public async Task BroadcastStopArrivalAsync(
        string tenantId,
        Guid tripId,
        Guid stopId,
        CancellationToken ct = default)
    {
        var stopArrival = new StopArrivalUpdateDto { TripId = tripId, StopId = stopId, ArrivedAt = DateTime.UtcNow };

        await hubContext.Clients
            .Groups(tenantId, $"trip:{tripId}")
            .ReceiveStopArrival(stopArrival);
    }

    public async Task BroadcastDispatchBoardUpdateAsync(
        string tenantId,
        CancellationToken ct = default)
    {
        // Create a generic dispatch board update notification
        // The actual update type and entity IDs should be provided by the caller when available
        var update = new DispatchBoardUpdateDto
        {
            UpdateType = DispatchBoardUpdateType.LoadAssigned, // Default type - should be provided by caller
            EntityId = Guid.Empty, // Should be provided by caller
            UpdatedAt = DateTime.UtcNow
        };

        await hubContext.Clients
            .Group($"dispatch-board:{tenantId}")
            .ReceiveDispatchBoardUpdate(update);
    }
}
