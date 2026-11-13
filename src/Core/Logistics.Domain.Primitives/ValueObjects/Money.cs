using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>An amount with its currency, for money that can arrive in a currency other than the tenant's. Amounts in the tenant currency are stored as plain decimals in <c>Tenant.Currency</c>.</summary>
[ComplexType]
public record Money
{
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public static Money Zero(string currency) => new() { Amount = 0M, Currency = currency };

    /// <summary>Formats as "$1,234.50" for USD and "1,234.50 EUR" otherwise - never in the server's culture.</summary>
    public string Format() => Format(Amount, Currency);

    /// <inheritdoc cref="Format()" />
    public static string Format(decimal amount, string currency) =>
        currency.Equals("USD", StringComparison.OrdinalIgnoreCase)
            ? amount.ToString("C", CultureInfo.GetCultureInfo("en-US"))
            : $"{amount.ToString("N2", CultureInfo.InvariantCulture)} {currency.ToUpperInvariant()}";
}
