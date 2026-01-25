using Logistics.Application.Commands;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Application.Hubs;

public class LiveTrackingHub(
    IMediator mediator,
    LiveTrackingHubContext hubContext) : Hub<ILiveTrackingHubClient>
{
    public override Task OnConnectedAsync()
    {
        hubContext.AddClient(Context.ConnectionId, null);
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var geolocationData = hubContext.GetGeolocationData(Context.ConnectionId);

        if (geolocationData != null)
        {
            await mediator.Send(new SetTruckGeolocationCommand(geolocationData));
        }

        hubContext.RemoveClient(Context.ConnectionId);
    }

    public async Task SendGeolocationData(TruckGeolocationDto truckGeolocation)
    {
        await Clients
            .Group(truckGeolocation.TenantId.ToString())
            .ReceiveGeolocationData(truckGeolocation);
        hubContext.UpdateGeolocationData(Context.ConnectionId, truckGeolocation);
    }

    public async Task RegisterTenant(string tenantId) => await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);

    public async Task UnregisterTenant(string tenantId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
}
