namespace Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;

/// <summary>
/// Samsara hosts each organization in one data region; a token only works against its own region's
/// host. The tenant picks the region on connect (<c>EldProviderConfiguration.Region</c>).
/// </summary>
public record SamsaraOptions
{
    public string BaseUrl { get; set; } = "https://api.samsara.com";
    public string EuBaseUrl { get; set; } = "https://api.eu.samsara.com";
    public string CaBaseUrl { get; set; } = "https://api.ca.samsara.com";

    public string BaseUrlFor(string? region) => region?.ToLowerInvariant() switch
    {
        "eu" => EuBaseUrl,
        "ca" => CaBaseUrl,
        _ => BaseUrl
    };
}
