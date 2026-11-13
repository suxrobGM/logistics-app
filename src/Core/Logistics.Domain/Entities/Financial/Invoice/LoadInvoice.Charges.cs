using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>Load-derived invoice lines (base rate, fuel surcharge, accessorials), rebuilt from the load; hand-added lines are kept.</summary>
public partial class LoadInvoice
{
    /// <summary>True for a line <see cref="ReplaceLoadCharges"/> owns and rebuilds.</summary>
    public static bool IsLoadCharge(InvoiceLineItem line) =>
        line.Type is InvoiceLineItemType.BaseRate or InvoiceLineItemType.FuelSurcharge || line.SourceId is not null;

    /// <summary>Rebuilds the load-derived lines from <paramref name="load"/> and returns the removed and added lines. The caller adds and removes them with the repository and recalculates totals.</summary>
    public (IReadOnlyList<InvoiceLineItem> Removed, IReadOnlyList<InvoiceLineItem> Added) ReplaceLoadCharges(Load load)
    {
        var added = BuildLoadCharges(load);
        return (ReplaceLines(IsLoadCharge, added), added);
    }

    private List<InvoiceLineItem> BuildLoadCharges(Load load)
    {
        var lines = new List<InvoiceLineItem>
        {
            Line(BaseRateDescription(load), InvoiceLineItemType.BaseRate, load.DeliveryCost, 1, null)
        };

        if (load.FuelSurcharge != 0)
        {
            lines.Add(Line("Fuel surcharge", InvoiceLineItemType.FuelSurcharge, load.FuelSurcharge, 1, null));
        }

        foreach (var accessorial in load.Accessorials.OrderBy(a => a.Name, StringComparer.Ordinal))
        {
            lines.Add(Line(accessorial.Name, accessorial.LineItemType, accessorial.Rate, accessorial.Quantity, accessorial.Id));
        }

        return lines;

        InvoiceLineItem Line(string description, InvoiceLineItemType type, decimal amount, decimal quantity, Guid? sourceId) => new()
        {
            InvoiceId = Id,
            Description = description,
            Type = type,
            Amount = amount,
            Quantity = quantity,
            SourceId = sourceId
        };
    }

    private static string BaseRateDescription(Load load) => load.Type switch
    {
        LoadType.Vehicle => "Vehicle transport charges",
        LoadType.RefrigeratedGoods => "Refrigerated transport charges",
        LoadType.HazardousMaterials => "Hazmat transport charges",
        _ => "Freight charges"
    };
}
