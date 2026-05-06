namespace Logistics.Infrastructure.Tax.Stripe;

/// <summary>
/// Stripe accepts and returns amounts as integer minor units (cents). Most ISO-4217 currencies
/// have 2 fractional digits; JPY/KRW are zero-decimal.
/// </summary>
internal static class StripeMoneyConversion
{
    private static readonly HashSet<string> ZeroDecimal =
        new(StringComparer.OrdinalIgnoreCase) { "JPY", "KRW", "VND", "CLP", "PYG" };

    public static long ToMinorUnits(decimal amount, string currency) =>
        IsZeroDecimal(currency)
            ? (long)decimal.Round(amount, 0, MidpointRounding.AwayFromZero)
            : (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

    public static decimal FromMinorUnits(long minor, string currency) =>
        IsZeroDecimal(currency) ? minor : minor / 100m;

    private static bool IsZeroDecimal(string currency) => ZeroDecimal.Contains(currency);
}
