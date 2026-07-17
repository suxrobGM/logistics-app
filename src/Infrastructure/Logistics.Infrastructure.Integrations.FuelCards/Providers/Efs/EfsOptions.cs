namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;

public class EfsOptions
{
    public const string SectionName = "FuelCards:Efs";

    public string BaseUrl { get; set; } = "https://api.efsllc.com";
}
