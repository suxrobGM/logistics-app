using Logistics.Application.Services;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

/// <summary>
///     Implementation of a trip tracking service using SignalR.
/// </summary>
internal sealed class SignalRTripTrackingService(IHubContext<TrackingHub, ITrackingHubClient> hubContext)
    : ITripTrackingService
{
    private const string TripGroupPrefix = "trip:";
    private const string DispatchBoardGroupPrefix = "dispatch-board:";

    public async Task BroadcastTripStatusUpdateAsync(Guid tenantId, TripStatusUpdateDto update)
    {
        // Broadcast to the specific trip's subscribers
        var tripGroup = $"{TripGroupPrefix}{update.TripId}";
        await hubContext.Clients.Group(tripGroup).ReceiveTripStatusUpdate(update);

        // Also broadcast to the tenant group for dashboard updates
        await hubContext.Clients.Group(tenantId.ToString()).ReceiveTripStatusUpdate(update);
    }

    public async Task BroadcastStopArrivalAsync(Guid tenantId, StopArrivalUpdateDto update)
    {
        // Broadcast to the specific trip's subscribers
        var tripGroup = $"{TripGroupPrefix}{update.TripId}";
        await hubContext.Clients.Group(tripGroup).ReceiveStopArrival(update);

        // Also broadcast to the tenant group
        await hubContext.Clients.Group(tenantId.ToString()).ReceiveStopArrival(update);
    }

    public async Task BroadcastDispatchBoardUpdateAsync(Guid tenantId, DispatchBoardUpdateDto update)
    {
        // Broadcast to dispatch board subscribers
        var dispatchBoardGroup = $"{DispatchBoardGroupPrefix}{tenantId}";
        await hubContext.Clients.Group(dispatchBoardGroup).ReceiveDispatchBoardUpdate(update);
    }
}
