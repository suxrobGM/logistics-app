namespace Logistics.Infrastructure.Integrations.Eld.Providers.Motive;

public record MotiveOptions
{
    public string BaseUrl { get; set; } = "https://api.keeptruckin.com/v1";
}
