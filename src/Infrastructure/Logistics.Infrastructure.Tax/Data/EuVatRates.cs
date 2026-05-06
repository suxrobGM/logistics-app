namespace Logistics.Infrastructure.Tax.Data;

/// <summary>
/// Standard VAT rates per EU member state (and EEA/UK), as of 2026-05.
/// Used by <c>ManualTaxCalculator</c> when no tenant-specific <c>TenantTaxRate</c> matches.
/// Stripe Tax is the recommended path for EU; this is a fallback so non-Stripe tenants don't crash.
/// </summary>
public static class EuVatRates
{
    public static readonly DateOnly LastUpdated = new(2026, 5, 1);

    public static readonly IReadOnlyDictionary<string, decimal> StandardRates =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        // EU-27 standard rates
        ["AT"] = 20.00m, ["BE"] = 21.00m, ["BG"] = 20.00m, ["HR"] = 25.00m,
        ["CY"] = 19.00m, ["CZ"] = 21.00m, ["DK"] = 25.00m, ["EE"] = 22.00m,
        ["FI"] = 25.50m, ["FR"] = 20.00m, ["DE"] = 19.00m, ["GR"] = 24.00m,
        ["HU"] = 27.00m, ["IE"] = 23.00m, ["IT"] = 22.00m, ["LV"] = 21.00m,
        ["LT"] = 21.00m, ["LU"] = 17.00m, ["MT"] = 18.00m, ["NL"] = 21.00m,
        ["PL"] = 23.00m, ["PT"] = 23.00m, ["RO"] = 19.00m, ["SK"] = 23.00m,
        ["SI"] = 22.00m, ["ES"] = 21.00m, ["SE"] = 25.00m,
        // EEA / non-EU European
        ["NO"] = 25.00m, ["IS"] = 24.00m, ["CH"] = 8.10m, ["LI"] = 8.10m,
        ["GB"] = 20.00m
    };

    public static decimal? GetStandardRate(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode)) return null;
        return StandardRates.TryGetValue(countryCode, out var rate) ? rate : null;
    }
}
