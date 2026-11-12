using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Logistics.Domain.Primitives.ValueObjects;

[ComplexType]
public record Money
{
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public static Money Zero(string currency) => new() { Amount = 0M, Currency = currency };

    /// <summary>Formats as "$1,234.50" for USD and "1,234.50 EUR" otherwise - never in the server's culture.</summary>
    public string Format() => Format(Amount, Currency);

    /// <inheritdoc cref="Format()" />
    public static string Format(decimal amount, string currency) =>
        currency.Equals("USD", StringComparison.OrdinalIgnoreCase)
            ? amount.ToString("C", CultureInfo.GetCultureInfo("en-US"))
            : $"{amount.ToString("N2", CultureInfo.InvariantCulture)} {currency.ToUpperInvariant()}";

    /// <summary>
    /// Implicitly converts a Money object to a decimal.
    /// </summary>
    /// <param name="entity">The Money object to convert.</param>
    /// <returns>The decimal amount of the Money object.</returns>
    public static implicit operator decimal(Money entity) => entity.Amount;
}
