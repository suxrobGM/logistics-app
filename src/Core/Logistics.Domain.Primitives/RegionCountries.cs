using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives;

/// <summary>
/// Maps a tenant <see cref="Region"/> to its allowed ISO 3166-1 alpha-2 country codes.
/// Used to validate that addresses match the tenant's operating region.
/// </summary>
public static class RegionCountries
{
    private static readonly HashSet<string> UsCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "US"
    };

    // Geographic Europe — broader than EU-27. Includes EEA/EFTA, UK, Western Balkans,
    // Eastern Europe (Moldova, Ukraine), and the microstates. Russia, Belarus, Turkey
    // intentionally omitted in v1; widen the set if a customer needs them.
    private static readonly HashSet<string> EuropeanCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        // EU-27
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE",
        "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE",
        // EEA / EFTA
        "IS", "LI", "NO", "CH",
        // United Kingdom
        "GB",
        // Western Balkans
        "AL", "BA", "ME", "MK", "RS", "XK",
        // Eastern Europe (selected)
        "MD", "UA",
        // Microstates
        "AD", "MC", "SM", "VA"
    };

    /// <summary>
    /// True when <paramref name="countryCode"/> is allowed for the given <paramref name="region"/>.
    /// </summary>
    public static bool IsAllowed(Region region, string? countryCode) =>
        region switch
        {
            Region.Us => UsCountries.Contains(countryCode ?? string.Empty),
            Region.Eu => EuropeanCountries.Contains(countryCode ?? string.Empty),
            _ => false
        };

    /// <summary>
    /// Returns the set of allowed ISO 3166-1 alpha-2 country codes for the region.
    /// </summary>
    public static IReadOnlyCollection<string> GetAllowed(Region region) =>
        region == Region.Us ? UsCountries : EuropeanCountries;
}
