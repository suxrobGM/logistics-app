using System.Text;
using Logistics.Client.Options;
using Logistics.DriverApp.Services.Authentication;
using Logistics.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Logistics.DriverApp.Services;

public class LocationTracker : ILocationTracker
{
    private readonly HubConnection _hubConnection;
    private readonly IAuthService _authService;
    private bool _isConnected;

    public LocationTracker(
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

    public async Task SendLocationDataAsync(string truckId)
    {
        await ConnectAsync();
        var tenantId = _authService.User?.CurrentTenantId;

        if (string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        var location = await GetCurrentLocationAsync();

        if (location is null)
        {
            return;
        }

        var address = await GetAddressFromGeocodeAsync(location.Latitude, location.Longitude);

        var geolocationData = new TruckGeolocationDto
        {
            TruckId = truckId,
            TenantId = tenantId,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            CurrentLocation = address
        };
        await _hubConnection.InvokeAsync("SendGeolocationData", geolocationData);
    }
    
    private static async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);
            return location;
        }
        catch (Exception)
        {
            // Unable to get location
            return null;
        }
    }

    private static async Task<string?> GetAddressFromGeocodeAsync(double latitude, double longitude)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);
            var placemark = placemarks?.FirstOrDefault();
        
            if (placemark != null)
            {
                return JoinNonEmptyStrings(", ",
                    $"{placemark.SubThoroughfare} {placemark.Thoroughfare}",
                    placemark.Locality,
                    placemark.SubAdminArea,
                    placemark.AdminArea,
                    placemark.PostalCode,
                    placemark.CountryName);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string JoinNonEmptyStrings(string separator, params string?[] strings)
    {
        var strBuilder = new StringBuilder();
        for (var i = 0; i < strings.Length; i++)
        {
            var str = strings[i];
            
            if (!string.IsNullOrEmpty(str))
            {
                strBuilder.Append(str);
            }

            if (i != strings.Length - 1)
            {
                strBuilder.Append(separator);
            }
        }

        return strBuilder.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
