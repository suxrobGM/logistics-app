namespace Logistics.Infrastructure.Services;

public record LoadBoardOptions
{
    public DatOptions? Dat { get; set; }
    public TruckstopOptions? Truckstop { get; set; }
    public OneTwo3LoadboardOptions? OneTwo3Loadboard { get; set; }
}

/// <summary>
/// DAT Load Board API configuration.
/// Authentication: Bearer Token (Service Account or User Account)
/// API Docs: https://freight.api.dat.com/docs
/// </summary>
public record DatOptions
{
    public string BaseUrl { get; set; } = "https://freight.api.dat.com";
    public string AuthUrl { get; set; } = "https://identity.api.dat.com/access/v1/token/organization";
}

/// <summary>
/// Truckstop.com Load Board API configuration.
/// Authentication: OAuth 2.0 (Resource Owner Password grant)
/// Access Token validity: 20 minutes
/// Refresh Token validity: 6 months
/// </summary>
public record TruckstopOptions
{
    public string BaseUrl { get; set; } = "https://api.truckstop.com";
    public string TokenUrl { get; set; } = "https://api.truckstop.com/auth/token";
}

/// <summary>
/// 123Loadboard API configuration.
/// Authentication: API Key (X-API-Key header)
/// Rate Limits: 100 searches/hour, 300/day, 2000/month
/// </summary>
public record OneTwo3LoadboardOptions
{
    public string BaseUrl { get; set; } = "https://api.123loadboard.com";
    public int MaxSearchesPerHour { get; set; } = 100;
    public int MaxSearchesPerDay { get; set; } = 300;
    public int MaxSearchesPerMonth { get; set; } = 2000;
}
