namespace Logistics.Infrastructure.Tax.Data;

/// <summary>
/// Minimal country-level GST/VAT defaults outside EU/US so the manual calculator never crashes
/// for non-EU/US tenants. Provincial layering (CA PST, AU state taxes) is intentionally
/// omitted — full coverage requires Stripe Tax.
/// </summary>
internal static class OtherCountryRates
{
    public static readonly DateOnly LastUpdated = new(2026, 5, 1);

    private static readonly Dictionary<string, decimal> Rates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AU"] = 10.00m, // GST
        ["NZ"] = 15.00m, // GST
        ["CA"] = 5.00m,  // GST only — provincial PST/HST not included
        ["JP"] = 10.00m, // Consumption tax
        ["SG"] = 9.00m,  // GST
        ["IN"] = 18.00m, // standard GST
        ["MX"] = 16.00m, // IVA
        ["BR"] = 17.00m, // ICMS (state-level avg)
        ["ZA"] = 15.00m  // VAT
    };

    public static decimal? GetRate(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode)) return null;
        return Rates.TryGetValue(countryCode, out var rate) ? rate : null;
    }
}
