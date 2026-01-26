using Logistics.Application.Hubs;
using Logistics.Application.Services;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Services;

/// <summary>
/// Implementation of trip tracking service using SignalR.
/// </summary>
public class TripTrackingService(IHubContext<LiveTrackingHub, ILiveTrackingHubClient> hubContext)
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
