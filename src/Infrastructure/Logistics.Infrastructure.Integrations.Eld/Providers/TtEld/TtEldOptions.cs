namespace Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;

public record TtEldOptions
{
    public string BaseUrl { get; set; } = "https://read.tteld.com";
}
