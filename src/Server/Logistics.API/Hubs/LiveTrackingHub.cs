using Microsoft.AspNetCore.SignalR;

namespace Logistics.API.Hubs;

public class LiveTrackingHub : Hub<ILiveTrackingClient>
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public Task SendGeolocationData(string userId, string tenantId, double latitude, double longitude)
    {
        // await Clients.Group(tenantId).ReceiveGeolocationData(userId, latitude, longitude);
        Console.WriteLine($"Received a geolocation data from: userId {userId}, tenantId: {tenantId}, latitude: {latitude}, longitude: {longitude}");
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
