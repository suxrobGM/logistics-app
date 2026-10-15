using System.Globalization;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// The lines a load invoice derives from its load: the linehaul, the fuel surcharge and one line per
/// accessorial. Everything else on the invoice (drayage charges, hand-added lines) is left alone.
/// </summary>
public partial class LoadInvoice
{
    /// <summary>True for a line <see cref="ReplaceLoadCharges"/> owns and rebuilds.</summary>
    public static bool IsLoadCharge(InvoiceLineItem line) =>
        line.Type is InvoiceLineItemType.BaseRate or InvoiceLineItemType.FuelSurcharge || line.SourceId is not null;

    /// <summary>
    /// Swaps the load-derived lines for fresh ones built from <paramref name="load"/>, first in the
    /// line order. The caller registers the removed and added lines with the repository and
    /// recalculates totals (tax applies per line).
    /// </summary>
    public (IReadOnlyList<InvoiceLineItem> Removed, IReadOnlyList<InvoiceLineItem> Added) ReplaceLoadCharges(Load load)
    {
        var removed = LineItems.Where(IsLoadCharge).ToList();
        foreach (var line in removed)
        {
            LineItems.Remove(line);
        }

        var added = BuildLoadCharges(load);
        var order = added.Count;
        foreach (var other in LineItems.OrderBy(li => li.Order))
        {
            other.Order = order++;
        }

        LineItems.AddRange(added);
        return (removed, added);
    }

    private List<InvoiceLineItem> BuildLoadCharges(Load load)
    {
        var currency = load.DeliveryCost.Currency;
        var lines = new List<InvoiceLineItem>
        {
            Line(BaseRateDescription(load), InvoiceLineItemType.BaseRate, load.DeliveryCost.Amount, 1, null)
        };

        if (load.FuelSurcharge != 0)
        {
            lines.Add(Line("Fuel surcharge", InvoiceLineItemType.FuelSurcharge, load.FuelSurcharge, 1, null));
        }

        foreach (var accessorial in load.Accessorials.OrderBy(a => a.Name, StringComparer.Ordinal))
        {
            lines.Add(AccessorialLine(accessorial));
        }

        for (var i = 0; i < lines.Count; i++)
        {
            lines[i].Order = i;
        }

        return lines;

        InvoiceLineItem Line(string description, InvoiceLineItemType type, decimal amount, int quantity, Guid? sourceId) => new()
        {
            InvoiceId = Id,
            Description = description,
            Type = type,
            Amount = new() { Amount = amount, Currency = currency },
            Quantity = quantity,
            SourceId = sourceId
        };

        // Line quantities are whole numbers; a fractional one (2.25 h) is billed as a single line
        // carrying the total, with the arithmetic in the description.
        InvoiceLineItem AccessorialLine(LoadAccessorial a)
        {
            if (a.Quantity == decimal.Truncate(a.Quantity) && a.Quantity is > 0 and <= int.MaxValue)
            {
                return Line(a.Name, a.LineItemType, a.Rate, (int)a.Quantity, a.Id);
            }

            var math = string.Create(CultureInfo.InvariantCulture, $"{a.Quantity:0.##} x {a.Rate:0.00}");
            return Line($"{a.Name} ({math})", a.LineItemType, a.Total, 1, a.Id);
        }
    }

    private static string BaseRateDescription(Load load) => load.Type switch
    {
        LoadType.Vehicle => "Vehicle transport charges",
        LoadType.RefrigeratedGoods => "Refrigerated transport charges",
        LoadType.HazardousMaterials => "Hazmat transport charges",
        _ => "Freight charges"
    };
}
