namespace Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;

public record SamsaraOptions
{
    public string BaseUrl { get; set; } = "https://api.samsara.com";
}
