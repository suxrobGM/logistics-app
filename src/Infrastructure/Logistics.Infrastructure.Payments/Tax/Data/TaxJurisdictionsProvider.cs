using Logistics.Application.Abstractions.Payments;
using Logistics.Domain.Primitives;

namespace Logistics.Infrastructure.Payments.Tax.Data;

internal sealed class TaxJurisdictionsProvider : ITaxJurisdictionsProvider
{
    private readonly Lazy<IReadOnlyList<TaxJurisdictionInfo>> _jurisdictions = new(BuildList);

    public IReadOnlyList<TaxJurisdictionInfo> GetSupportedJurisdictions() => _jurisdictions.Value;

    private static IReadOnlyList<TaxJurisdictionInfo> BuildList()
    {
        var result = new List<TaxJurisdictionInfo>();

        foreach (var country in RegionCountries.EUMemberStates.Concat(["NO", "IS", "CH", "LI", "GB"]))
        {
            result.Add(new TaxJurisdictionInfo(
                country, null, country,
                EUVatRates.GetStandardRate(country),
                "eu_vat"));
        }

        foreach (var state in USSalesTaxRates.StateCodes)
        {
            result.Add(new TaxJurisdictionInfo(
                "US", state, $"US-{state}",
                USSalesTaxRates.GetStateBaseRate(state),
                "us_sales_tax"));
        }

        foreach (var country in new[] { "AU", "NZ", "CA", "JP", "SG", "IN", "MX", "BR", "ZA" })
        {
            result.Add(new TaxJurisdictionInfo(
                country, null, country,
                OtherCountryRates.GetRate(country),
                "country_default"));
        }

        return result;
    }
}
