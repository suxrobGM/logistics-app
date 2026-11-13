using System.Text.Json;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Tax-related behavior for <see cref="Invoice"/>: totals math, breakdown JSON helpers,
/// and the <c>InvoiceTotalsRecalculatedEvent</c>.
/// </summary>
public abstract partial class Invoice
{
    private static readonly JsonSerializerOptions BreakdownJsonOptions = new(JsonSerializerDefaults.Web);

    public IReadOnlyList<InvoiceTaxLine> GetTaxBreakdown()
    {
        if (string.IsNullOrWhiteSpace(TaxBreakdownJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<InvoiceTaxLine>>(TaxBreakdownJson, BreakdownJsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public void SetTaxBreakdown(IEnumerable<InvoiceTaxLine> breakdown)
    {
        var list = breakdown as IList<InvoiceTaxLine> ?? breakdown.ToList();
        TaxBreakdownJson = list.Count == 0 ? null : JsonSerializer.Serialize(list, BreakdownJsonOptions);
    }

    /// <summary>
    /// Recomputes <see cref="Subtotal"/>, <see cref="TaxTotal"/>, and <see cref="Total"/>
    /// from current line items per the active <see cref="TaxBehavior"/>, then the payment status when the
    /// invoice has payments. Idempotent. Raises <see cref="InvoiceTotalsRecalculatedEvent"/>.
    /// </summary>
    public void RecalculateTotals()
    {
        decimal subtotal;
        decimal taxTotal;
        decimal total;

        switch (TaxBehavior)
        {
            case TaxBehavior.Inclusive:
                // Line `Total` already includes tax. Back out the net per-line.
                subtotal = LineItems.Sum(li =>
                {
                    var rateFactor = 1m + (li.TaxRatePercent / 100m);
                    return rateFactor == 0m ? li.Total : decimal.Round(li.Total / rateFactor, 2);
                });
                total = LineItems.Sum(li => li.Total);
                taxTotal = total - subtotal;
                break;

            case TaxBehavior.ReverseCharge:
                // No VAT collected; per-line tax is forced to zero.
                foreach (var li in LineItems)
                {
                    li.TaxAmount = 0m;
                }
                subtotal = LineItems.Sum(li => li.Total);
                taxTotal = 0m;
                total = subtotal;
                break;

            case TaxBehavior.Exclusive:
            default:
                subtotal = LineItems.Sum(li => li.Total);
                taxTotal = LineItems.Sum(li => li.TaxAmount);
                total = subtotal + taxTotal;
                break;
        }

        Subtotal = decimal.Round(subtotal, 2);
        TaxTotal = decimal.Round(taxTotal, 2);
        Total = decimal.Round(total, 2);

        if (Payments.Count > 0)
        {
            RecalculatePaymentStatus(DateTime.UtcNow);
        }

        DomainEvents.Add(new InvoiceTotalsRecalculatedEvent(
            Id,
            Subtotal,
            TaxTotal,
            Total,
            TaxBehavior));
    }
}
