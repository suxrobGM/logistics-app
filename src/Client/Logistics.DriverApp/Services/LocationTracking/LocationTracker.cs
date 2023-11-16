using Logistics.Client.Options;
using Logistics.DriverApp.Services.Authentication;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Logistics.DriverApp.Services.LocationTracking;

public class LocationTracker : ILocationTracker
{
    private readonly HubConnection _hubConnection;
    private bool _isConnected;

    public LocationTracker(ApiClientOptions apiClientOptions)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{apiClientOptions.Host}/hubs/live-tracking", options =>
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

    public async Task<Location?> SendLocationDataAsync(LocationTrackerOptions options)
    {
        try
        {
            if (string.IsNullOrEmpty(options.TruckId) || string.IsNullOrEmpty(options.TenantId))
            {
                return default;
            }
        
            await ConnectAsync();
            var location = await GetCurrentLocationAsync();

            if (location is null)
            {
                return default;
            }

            var address = await GetAddressFromGeocodeAsync(location.Latitude, location.Longitude);
            
            var geolocationData = new TruckGeolocationDto
            {
                TruckId = options.TruckId,
                TruckNumber = options.TruckNumber,
                TenantId = options.TenantId,
                DriversName = options.DriversName,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                CurrentAddress = address
            };
            await _hubConnection.InvokeAsync("SendGeolocationData", geolocationData);
            return location;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return default;
        }
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

    private static async Task<AddressDto?> GetAddressFromGeocodeAsync(double latitude, double longitude)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);
            var placemark = placemarks.FirstOrDefault();
        
            if (placemark != null)
            {
                return new AddressDto
                {
                    Line1 = placemark.SubThoroughfare,
                    Line2 = placemark.Thoroughfare,
                    City = placemark.Locality,
                    Region = placemark.AdminArea,
                    ZipCode = placemark.PostalCode,
                    Country = placemark.CountryName
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
