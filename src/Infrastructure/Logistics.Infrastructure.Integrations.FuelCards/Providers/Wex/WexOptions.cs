namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Wex;

public class WexOptions
{
    public const string SectionName = "FuelCards:Wex";

    public string BaseUrl { get; set; } = "https://api.wexinc.com";
}
