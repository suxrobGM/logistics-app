using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Infrastructure.Integrations.FuelCards.Common;

internal static class JurisdictionMapper
{
    private static readonly HashSet<string> CanadianProvinces =
    [
        "AB", "BC", "MB", "NB", "NL", "NS", "NT", "NU", "ON", "PE", "QC", "SK", "YT"
    ];

    /// <summary>
    /// Builds a jurisdiction from provider merchant fields. Providers often send only a
    /// state/province code; when the country is missing it is inferred (Canadian province
    /// codes → CA, otherwise US — WEX/EFS/Comdata are North-America networks).
    /// </summary>
    public static TaxJurisdiction? FromMerchant(string? country, string? stateOrProvince)
    {
        var region = stateOrProvince?.Trim().ToUpperInvariant();
        var countryCode = country?.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(region) && string.IsNullOrEmpty(countryCode))
        {
            return null;
        }

        countryCode = countryCode switch
        {
            "USA" => "US",
            "CAN" => "CA",
            "MEX" => "MX",
            null or "" => region is not null && CanadianProvinces.Contains(region) ? "CA" : "US",
            _ => countryCode
        };

        return new TaxJurisdiction { CountryCode = countryCode, Region = region };
    }
}
