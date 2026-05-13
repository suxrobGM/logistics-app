namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

/// <summary>
///     DAT Load Board API configuration.
///     Authentication: Bearer Token (Service Account or User Account)
///     API Docs: https://freight.api.dat.com/docs
/// </summary>
public record DatOptions
{
    public string BaseUrl { get; set; } = "https://freight.api.dat.com";
    public string AuthUrl { get; set; } = "https://identity.api.dat.com/access/v1/token/organization";
}
