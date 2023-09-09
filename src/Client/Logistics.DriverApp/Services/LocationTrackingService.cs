using Logistics.Client.Options;
using Logistics.DriverApp.Services.Authentication;
using Logistics.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Logistics.DriverApp.Services;

public class LocationTrackingService : ILocationTrackingService
{
    private readonly HubConnection _hubConnection;
    private readonly IAuthService _authService;
    private bool _isConnected;

    public LocationTrackingService(
        ApiClientOptions apiClientOptions,
        IAuthService authService)
    {
        _authService = authService;
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{apiClientOptions.Host}/hubs/LiveTracking", options =>
            {
#if DEBUG
                // bypass self-signed certs
                options.HttpMessageHandlerFactory = (_) => InsecureHttpsClient.GetPlatformMessageHandler();
#endif
            })
            .Build();
    }

    private async Task ConnectAsync()
    {
        if (_isConnected)
            return;
        
        await _hubConnection.StartAsync();
        _isConnected = true;
    }

    private async Task DisconnectAsync()
    {
        if (!_isConnected)
            return;
        
        await _hubConnection.DisposeAsync();
        _isConnected = false;
    }

    public async Task SendLocationDataAsync()
    {
        await ConnectAsync();
        var userId = _authService.User?.Id;
        var tenantId = _authService.User?.CurrentTenantId;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        var location = await GetCurrentLocationAsync();

        if (location is null)
        {
            return;
        }

        var geolocationData = new GeolocationData
        {
            UserId = userId,
            TenantId = tenantId,
            Latitude = location.Latitude,
            Longitude = location.Longitude
        };
        await _hubConnection.InvokeAsync("SendGeolocationData", geolocationData);
    }
    
    private static async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is not null)
            {
                Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
            }

            return location;
        }
        catch (Exception)
        {
            // Unable to get location
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
