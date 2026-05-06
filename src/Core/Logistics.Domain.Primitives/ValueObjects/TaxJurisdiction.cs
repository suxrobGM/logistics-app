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

    public static TaxJurisdiction ForCountry(string countryCode) => new() { CountryCode = countryCode };

    public override string ToString() =>
        string.IsNullOrEmpty(Region) ? CountryCode : $"{CountryCode}-{Region}";
}
