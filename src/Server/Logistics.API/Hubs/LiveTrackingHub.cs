using Logistics.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.API.Hubs;

public class LiveTrackingHub : Hub<ILiveTrackingHubClient>
{
    private readonly IMediator _mediator;
    private readonly LiveTrackingClientsContext _clientsContext;

    public LiveTrackingHub(
        IMediator mediator, 
        LiveTrackingClientsContext clientsContext)
    {
        _mediator = mediator;
        _clientsContext = clientsContext;
    }
    
    public override Task OnConnectedAsync()
    {
        _clientsContext.AddClient(Context.ConnectionId, null);
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var geolocationData = _clientsContext.GetGeolocationData(Context.ConnectionId);
        await _mediator.Send(new SaveEmployeeGeolocationCommand(geolocationData));
        _clientsContext.RemoveClient(Context.ConnectionId);
    }

    public Task SendGeolocationData(GeolocationData geolocation)
    {
        // await Clients.Group(tenantId).ReceiveGeolocationData(userId, latitude, longitude);
        Console.WriteLine(
            $"Received a geolocation data from: userId {geolocation.UserId}, tenantId: {geolocation.TenantId}, latitude: {geolocation.Latitude}, longitude: {geolocation.Longitude}");
        
        _clientsContext.UpdateData(Context.ConnectionId, geolocation);
        return Task.CompletedTask;
    }
    
    public async Task RegisterTenant(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
        // await Clients.Group(tenantId).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
    }

    public async Task UnregisterTenant(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
        // await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
    }
}
