using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// What the load bills beyond the linehaul (<see cref="Load.DeliveryCost"/>).
/// </summary>
public partial class Load
{
    /// <summary>Flat fuel surcharge in the load currency; 0 when the rate is all-in.</summary>
    public decimal FuelSurcharge { get; set; }

    public virtual List<LoadAccessorial> Accessorials { get; set; } = [];

    /// <summary>The detention line written for a stop, if any.</summary>
    public LoadAccessorial? FindDetention(Guid tripStopId) =>
        Accessorials.FirstOrDefault(a => a.TripStopId == tripStopId && a.LineItemType == InvoiceLineItemType.Detention);
}
