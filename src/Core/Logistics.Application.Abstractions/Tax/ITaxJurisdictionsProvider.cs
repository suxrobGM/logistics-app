using Logistics.Application.Abstractions.Tax;
namespace Logistics.Application.Abstractions.Tax;

/// <summary>
/// Returns the static set of supported jurisdictions (country + optional region) backed by the
/// EU VAT, US sales tax, and other-country fallback tables in <c>Logistics.Infrastructure.Tax</c>.
/// Used to populate the manual-rate UI's jurisdiction dropdown.
/// </summary>
public interface ITaxJurisdictionsProvider
{
    IReadOnlyList<TaxJurisdictionInfo> GetSupportedJurisdictions();
}

public sealed record TaxJurisdictionInfo(
    string CountryCode,
    string? Region,
    string DisplayName,
    decimal? DefaultRatePercent,
    string Source);
