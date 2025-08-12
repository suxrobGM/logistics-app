using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Primitives.ValueObjects;

[ComplexType]
public record Money
{
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public static Money Zero(string currency = "USD") => new() { Amount = 0M, Currency = currency };

    /// <summary>
    /// Implicitly converts a Money object to a decimal.
    /// </summary>
    /// <param name="entity">The Money object to convert.</param>
    /// <returns>The decimal amount of the Money object.</returns>
    public static implicit operator decimal(Money entity) => entity.Amount;

    /// <summary>
    /// Implicitly converts a decimal to a Money object with the default currency.
    /// </summary>
    /// <param name="amount">The decimal amount to convert.</param>
    /// <returns>A Money object with the specified amount and default currency.</returns>
    public static implicit operator Money(decimal amount) => new() { Amount = amount, Currency = "USD" };
}
