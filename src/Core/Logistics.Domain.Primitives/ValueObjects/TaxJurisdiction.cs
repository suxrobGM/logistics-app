using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// Identifies the geographic scope a tax rate applies to.
/// CountryCode is ISO 3166-1 alpha-2; Region is a state/province/Bundesland code (optional).
/// </summary>
[ComplexType]
public record TaxJurisdiction
{
    public required string CountryCode { get; set; }
    public string? Region { get; set; }

    /// <summary>
    /// Normalizing factory — use this on every write path. Codes are compared as upper-case, and a
    /// blank region means "country-level", so it must normalize to null rather than an empty string.
    /// </summary>
    public static TaxJurisdiction Create(string countryCode, string? region = null) => new()
    {
        CountryCode = countryCode.ToUpperInvariant(),
        Region = string.IsNullOrWhiteSpace(region) ? null : region.ToUpperInvariant()
    };

    public static TaxJurisdiction ForCountry(string countryCode) => Create(countryCode);

    public override string ToString() =>
        string.IsNullOrEmpty(Region) ? CountryCode : $"{CountryCode}-{Region}";
}
