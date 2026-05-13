namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;

/// <summary>
///     123Loadboard API configuration.
///     Authentication: API Key (X-API-Key header)
///     Rate Limits: 100 searches/hour, 300/day, 2000/month
/// </summary>
public record OneTwo3LoadboardOptions
{
    public string BaseUrl { get; set; } = "https://api.123loadboard.com";
    public int MaxSearchesPerHour { get; set; } = 100;
    public int MaxSearchesPerDay { get; set; } = 300;
    public int MaxSearchesPerMonth { get; set; } = 2000;
}
