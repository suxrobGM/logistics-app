namespace Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;

/// <summary>
/// Options for the Geotab MyGeotab integration. <see cref="BaseUrl"/> is the
/// initial server hint; MyGeotab redirects to a federated server (e.g. my3.geotab.com)
/// returned by the authentication call.
/// </summary>
public record GeotabOptions
{
    public string BaseUrl { get; set; } = "https://my.geotab.com";
}
