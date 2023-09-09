using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.API.Hubs;

public class LiveTrackingHub : Hub<ILiveTrackingClient>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly LiveTrackingClientsContext _clientsContext;

    public LiveTrackingHub(
        ITenantRepository tenantRepository, 
        LiveTrackingClientsContext clientsContext)
    {
        _tenantRepository = tenantRepository;
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

        await SaveGeolocationDataAsync(geolocationData);
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

    private async Task SaveGeolocationDataAsync(GeolocationData? geolocation)
    {
        if (geolocation is null)
            return;
        
        Console.WriteLine("Saving a geolocation to the database");
        _tenantRepository.SetTenantId(geolocation.TenantId);
        var employee = await _tenantRepository.GetAsync<Employee>(geolocation.UserId);

        if (employee is not null)
        {
            employee.LastKnownLocation = $"{geolocation.Latitude},{geolocation.Longitude}";
            _tenantRepository.Update(employee);
            await _tenantRepository.UnitOfWork.CommitAsync();
            Console.WriteLine("Geolocation data has been saved");
        }
    }
}
