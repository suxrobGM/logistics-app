using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Settlement lines: gross pay first, then one negative line per deduction, so <see cref="Invoice.Total"/>
/// is the net. Hand-added lines (one-off deductions, reimbursements, bonuses) are kept on a rebuild.
/// </summary>
public partial class PayrollInvoice
{
    /// <summary>True for a line <see cref="ReplaceSettlementLines"/> owns and rebuilds.</summary>
    public static bool IsGeneratedLine(InvoiceLineItem line) =>
        line.Type == InvoiceLineItemType.BasePay || line.SourceId is not null;

    /// <summary>
    /// The stored amount for a hand-entered settlement line: deductions are always negative whatever
    /// sign was typed. Escrow keeps its sign - negative holds money back, positive releases it.
    /// </summary>
    public static decimal SignedLineAmount(InvoiceLineItemType type, decimal amount) =>
        type.IsPayDeduction() && type != InvoiceLineItemType.Escrow ? -Math.Abs(amount) : amount;

    /// <summary>
    /// Swaps the generated lines for ones built from <paramref name="inputs"/> and recalculates the
    /// totals. The caller registers the removed and added lines with the repository.
    /// </summary>
    public (IReadOnlyList<InvoiceLineItem> Removed, IReadOnlyList<InvoiceLineItem> Added) ReplaceSettlementLines(
        SettlementInputs inputs)
    {
        var removed = LineItems.Where(IsGeneratedLine).ToList();
        foreach (var line in removed)
        {
            LineItems.Remove(line);
        }

        var added = BuildSettlementLines(inputs);
        var order = added.Count;
        foreach (var other in LineItems.OrderBy(li => li.Order))
        {
            other.Order = order++;
        }

        LineItems.AddRange(added);
        RecalculateTotals();
        return (removed, added);
    }

    private List<InvoiceLineItem> BuildSettlementLines(SettlementInputs inputs)
    {
        var lines = new List<InvoiceLineItem>
        {
            Line(inputs.GrossPayDescription, InvoiceLineItemType.BasePay, decimal.Round(inputs.GrossPay, 2), null)
        };

        var escrowBalance = inputs.EscrowBalance;
        foreach (var deduction in inputs.RecurringDeductions.Where(d => d.Amount > 0))
        {
            if (deduction.Type != PayDeductionType.Escrow)
            {
                lines.Add(Line(deduction.Description, InvoiceLineItemType.Deduction, -deduction.Amount, deduction.Id));
                continue;
            }

            var contribution = deduction.EscrowTarget is { } target
                ? Math.Min(deduction.Amount, Math.Max(0, target - escrowBalance))
                : deduction.Amount;

            if (contribution > 0)
            {
                escrowBalance += contribution;
                lines.Add(Line(deduction.Description, InvoiceLineItemType.Escrow, -contribution, deduction.Id));
            }
        }

        foreach (var advance in inputs.CashAdvances.OrderBy(a => a.CreatedAt))
        {
            var reason = string.IsNullOrWhiteSpace(advance.Reason) ? "" : $" - {advance.Reason}";
            lines.Add(Line($"Cash advance {advance.CreatedAt:MM/dd}{reason}", InvoiceLineItemType.CashAdvance,
                -advance.Amount.Amount, advance.Id));
        }

        foreach (var fuel in inputs.FuelCharges.OrderBy(f => f.TransactionDate))
        {
            var merchant = string.Join(", ", new[] { fuel.MerchantName, fuel.MerchantCity }.Where(s => !string.IsNullOrWhiteSpace(s)));
            var where = merchant.Length == 0 ? "" : $" - {merchant}";
            lines.Add(Line($"Fuel {fuel.TransactionDate:MM/dd}{where}", InvoiceLineItemType.FuelCard,
                -fuel.Amount.Amount, fuel.Id));
        }

        for (var i = 0; i < lines.Count; i++)
        {
            lines[i].Order = i;
        }

        return lines;

        InvoiceLineItem Line(string description, InvoiceLineItemType type, decimal amount, Guid? sourceId) => new()
        {
            InvoiceId = Id,
            Description = description,
            Type = type,
            Amount = new() { Amount = amount, Currency = Total.Currency },
            Quantity = 1,
            SourceId = sourceId
        };
    }
}
