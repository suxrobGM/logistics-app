namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;

/// <summary>
///     EFS card program API configuration. Bound as <see cref="FuelCardsOptions.Efs"/>.
/// </summary>
public record EfsOptions
{
    public string BaseUrl { get; set; } = "https://api.efsllc.com";
}
