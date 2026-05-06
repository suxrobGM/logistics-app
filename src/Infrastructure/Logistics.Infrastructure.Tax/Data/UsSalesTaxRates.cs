namespace Logistics.Infrastructure.Tax.Data;

/// <summary>
/// US state-level base sales-tax rates as of 2026-05.
/// Local (county/city) rates are NOT included — accurate destination-based US tax requires
/// Stripe Tax. The manual calculator surfaces a warning so the UI can flag this.
/// Origin: Tax Foundation / state DOR sites.
/// </summary>
internal static class UsSalesTaxRates
{
    public static readonly DateOnly LastUpdated = new(2026, 5, 1);

    private static readonly Dictionary<string, decimal> StateBaseRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AL"] = 4.00m, ["AK"] = 0.00m, ["AZ"] = 5.60m, ["AR"] = 6.50m, ["CA"] = 7.25m,
        ["CO"] = 2.90m, ["CT"] = 6.35m, ["DE"] = 0.00m, ["FL"] = 6.00m, ["GA"] = 4.00m,
        ["HI"] = 4.00m, ["ID"] = 6.00m, ["IL"] = 6.25m, ["IN"] = 7.00m, ["IA"] = 6.00m,
        ["KS"] = 6.50m, ["KY"] = 6.00m, ["LA"] = 4.45m, ["ME"] = 5.50m, ["MD"] = 6.00m,
        ["MA"] = 6.25m, ["MI"] = 6.00m, ["MN"] = 6.875m, ["MS"] = 7.00m, ["MO"] = 4.225m,
        ["MT"] = 0.00m, ["NE"] = 5.50m, ["NV"] = 6.85m, ["NH"] = 0.00m, ["NJ"] = 6.625m,
        ["NM"] = 4.875m, ["NY"] = 4.00m, ["NC"] = 4.75m, ["ND"] = 5.00m, ["OH"] = 5.75m,
        ["OK"] = 4.50m, ["OR"] = 0.00m, ["PA"] = 6.00m, ["RI"] = 7.00m, ["SC"] = 6.00m,
        ["SD"] = 4.20m, ["TN"] = 7.00m, ["TX"] = 6.25m, ["UT"] = 6.10m, ["VT"] = 6.00m,
        ["VA"] = 5.30m, ["WA"] = 6.50m, ["WV"] = 6.00m, ["WI"] = 5.00m, ["WY"] = 4.00m,
        ["DC"] = 6.00m
    };

    public static decimal? GetStateBaseRate(string? stateCode)
    {
        if (string.IsNullOrEmpty(stateCode)) return null;
        return StateBaseRates.TryGetValue(stateCode, out var rate) ? rate : null;
    }
}
