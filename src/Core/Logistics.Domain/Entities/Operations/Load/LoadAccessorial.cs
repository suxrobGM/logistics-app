using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// An extra charge billed on a load: picked from the tenant catalog, typed in ad hoc, or written
/// by detention when a truck leaves a stop past its free time. Each one becomes one invoice line.
/// </summary>
public class LoadAccessorial : Entity, ITenantEntity
{
    public Guid LoadId { get; set; }
    public virtual Load Load { get; set; } = null!;

    /// <summary>The catalog entry it was copied from, if any. Kept for reporting only.</summary>
    public Guid? AccessorialChargeId { get; set; }

    /// <summary>Set on detention lines: the stop the truck waited at. One detention per stop.</summary>
    public Guid? TripStopId { get; set; }

    public required string Name { get; set; }
    public required InvoiceLineItemType LineItemType { get; set; }
    public required AccessorialUnit Unit { get; set; }

    /// <summary>Rate per <see cref="Unit"/> in the load currency.</summary>
    public decimal Rate { get; set; }

    /// <summary>Hours, miles/km or stops for per-unit charges; 1 for flat ones.</summary>
    public decimal Quantity { get; set; } = 1;

    public decimal Total => decimal.Round(Rate * Quantity, 2);
}
