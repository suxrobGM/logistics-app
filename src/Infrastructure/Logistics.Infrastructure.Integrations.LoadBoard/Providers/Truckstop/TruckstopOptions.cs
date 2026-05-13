namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Truckstop;

/// <summary>
///     Truckstop.com Load Board API configuration.
///     Authentication: OAuth 2.0 (Resource Owner Password grant)
///     Access Token validity: 20 minutes
///     Refresh Token validity: 6 months
/// </summary>
public record TruckstopOptions
{
    public string BaseUrl { get; set; } = "https://api.truckstop.com";
    public string TokenUrl { get; set; } = "https://api.truckstop.com/auth/token";
}
