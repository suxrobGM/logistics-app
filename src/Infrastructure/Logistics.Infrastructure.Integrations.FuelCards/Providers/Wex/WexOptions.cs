namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Wex;

/// <summary>
///     WEX fleet card API configuration. Bound as <see cref="FuelCardsOptions.Wex"/>.
/// </summary>
public record WexOptions
{
    public string BaseUrl { get; set; } = "https://api.wexinc.com";
}
