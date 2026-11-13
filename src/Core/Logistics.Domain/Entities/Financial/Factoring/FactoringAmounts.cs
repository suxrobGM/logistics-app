namespace Logistics.Domain.Entities;

/// <summary>
/// How a factored invoice's total splits: cash advanced at funding, the factor's fee, and the reserve held
/// back until the customer pays the factor. The three always add up to the invoice total.
/// </summary>
public sealed record FactoringAmounts(decimal Advance, decimal Fee, decimal Reserve)
{
    public static FactoringAmounts FromAdvanceAndFee(decimal invoiceTotal, decimal advance, decimal fee)
    {
        if (advance < 0m || fee < 0m)
        {
            throw new InvalidOperationException("The advance and the fee cannot be negative.");
        }

        var reserve = invoiceTotal - advance - fee;
        if (reserve < 0m)
        {
            throw new InvalidOperationException(
                $"The advance and the fee ({advance + fee:0.00}) cannot exceed the invoice total ({invoiceTotal:0.00}).");
        }

        return new FactoringAmounts(advance, fee, reserve);
    }
}
