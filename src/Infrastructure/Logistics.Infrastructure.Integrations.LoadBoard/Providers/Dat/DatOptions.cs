namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;

/// <summary>
/// DAT API hosts. DAT's developer portal (developer.dat.com) is gated; every value here comes
/// from public client code, and the ones marked assumed must be checked against the portal once
/// a carrier's API access is approved. Staging hosts insert <c>.staging</c>
/// (<c>identity.api.staging.dat.com</c>, <c>analytics.api.staging.dat.com</c>).
/// </summary>
public record DatOptions
{
    /// <summary>
    /// Organization and user token endpoints live under <c>/access/v1/token/*</c> (verified).
    /// </summary>
    public string IdentityBaseUrl { get; set; } = "https://identity.api.dat.com";

    /// <summary>
    /// Posting API, <c>/posting/v2/...</c> (verified for loads; trucks assumed to mirror it).
    /// </summary>
    public string FreightBaseUrl { get; set; } = "https://freight.api.dat.com";

    /// <summary>
    /// Freight-matching search, <c>/search/v1/loads</c> (verified in 2021 client code; DAT now
    /// documents a v3 whose shape is not public).
    /// </summary>
    public string SearchBaseUrl { get; set; } = "https://freight.api.prod.dat.com";

    /// <summary>
    /// RateView lane rates, <c>/linehaulrates/v1/lookups</c> (verified).
    /// </summary>
    public string AnalyticsBaseUrl { get; set; } = "https://analytics.api.dat.com";

    /// <summary>
    /// Oldest posting a search returns, in minutes.
    /// </summary>
    public int MaxPostingAgeMinutes { get; set; } = 1440;
}
