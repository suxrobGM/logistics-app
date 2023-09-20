using Logistics.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.API.Hubs;

public class LiveTrackingHub : Hub<ILiveTrackingHubClient>
{
    private readonly IMediator _mediator;
    private readonly LiveTrackingHubContext _hubContext;

    public LiveTrackingHub(
        IMediator mediator, 
        LiveTrackingHubContext hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }
    
    public override Task OnConnectedAsync()
    {
        _hubContext.AddClient(Context.ConnectionId, null);
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var geolocationData = _hubContext.GetGeolocationData(Context.ConnectionId);
        await _mediator.Send(new SetTruckGeolocationCommand(geolocationData));
        _hubContext.RemoveClient(Context.ConnectionId);
    }

    public async Task SendGeolocationData(TruckGeolocationDto truckGeolocation)
    {
        Console.WriteLine(
            $"Received a geolocation data from: truckId {truckGeolocation}, tenantId: {truckGeolocation.TenantId}, latitude: {truckGeolocation.Latitude}, longitude: {truckGeolocation.Longitude}");
        
        await Clients.Group(truckGeolocation.TenantId).ReceiveGeolocationData(truckGeolocation);
        _hubContext.UpdateGeolocationData(Context.ConnectionId, truckGeolocation);
    }
    
    public async Task RegisterTenant(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
    }

    public async Task UnregisterTenant(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
    }
}
