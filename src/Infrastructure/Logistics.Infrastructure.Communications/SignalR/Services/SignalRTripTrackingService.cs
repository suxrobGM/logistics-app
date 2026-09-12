using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Logistics.Application.Abstractions.Routing;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

/// <summary>
///     Implementation of a trip tracking service using SignalR.
/// </summary>
internal sealed class SignalRTripTrackingService(IHubContext<TrackingHub, ITrackingHubClient> hubContext)
    : ITripTrackingService
{
    public async Task BroadcastTripStatusUpdateAsync(Guid tenantId, TripStatusUpdateDto update)
    {
        await hubContext.Clients.Group(tenantId.ToString()).ReceiveTripStatusUpdate(update);
    }

    public async Task BroadcastStopArrivalAsync(Guid tenantId, StopArrivalUpdateDto update)
    {
        await hubContext.Clients.Group(tenantId.ToString()).ReceiveStopArrival(update);
    }

    public async Task BroadcastDispatchBoardUpdateAsync(Guid tenantId, DispatchBoardUpdateDto update)
    {
        var dispatchBoardGroup = AIDispatchHub.GroupName(tenantId);
        await hubContext.Clients.Group(dispatchBoardGroup).ReceiveDispatchBoardUpdate(update);
    }

}
