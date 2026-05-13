using Logistics.Application.Abstractions.Tax;

namespace Logistics.Infrastructure.Tax.Data;

internal sealed class TaxJurisdictionsProvider : ITaxJurisdictionsProvider
{
    private readonly Lazy<IReadOnlyList<TaxJurisdictionInfo>> jurisdictions = new(BuildList);

    public IReadOnlyList<TaxJurisdictionInfo> GetSupportedJurisdictions() => jurisdictions.Value;

    private static IReadOnlyList<TaxJurisdictionInfo> BuildList()
    {
        var result = new List<TaxJurisdictionInfo>();

        foreach (var country in EuVatRules.EuMemberStates.Concat(["NO", "IS", "CH", "LI", "GB"]))
        {
            result.Add(new TaxJurisdictionInfo(
                country, null, country,
                EuVatRates.GetStandardRate(country),
                "eu_vat"));
        }

        foreach (var (state, rate) in UsStateNames())
        {
            result.Add(new TaxJurisdictionInfo("US", state, $"US-{state}", rate, "us_sales_tax"));
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

    private static IEnumerable<(string State, decimal? Rate)> UsStateNames()
    {
        string[] states =
        [
            "AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA","HI","ID","IL","IN","IA",
            "KS","KY","LA","ME","MD","MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ",
            "NM","NY","NC","ND","OH","OK","OR","PA","RI","SC","SD","TN","TX","UT","VT",
            "VA","WA","WV","WI","WY","DC"
        ];
        return states.Select(s => (s, UsSalesTaxRates.GetStateBaseRate(s)));
    }
}
