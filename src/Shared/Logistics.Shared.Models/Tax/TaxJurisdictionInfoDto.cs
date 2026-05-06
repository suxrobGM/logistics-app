namespace Logistics.Shared.Models;

public record TaxJurisdictionInfoDto
{
    public required string CountryCode { get; init; }
    public string? Region { get; init; }
    public required string DisplayName { get; init; }
    public decimal? DefaultRatePercent { get; init; }
    public required string Source { get; init; }
}
