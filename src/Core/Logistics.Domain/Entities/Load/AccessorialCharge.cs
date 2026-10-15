using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// A tenant's price list entry for an extra charge (lumper, layover, liftgate, extra stop, ...).
/// Adding one to a load copies its values, so a later price change never rewrites old invoices.
/// </summary>
public class AccessorialCharge : Entity, ITenantEntity
{
    public required string Name { get; set; }

    /// <summary>The invoice line type the charge is billed as.</summary>
    public required InvoiceLineItemType LineItemType { get; set; }

    public required AccessorialUnit Unit { get; set; }

    /// <summary>Rate per <see cref="Unit"/> in the tenant currency.</summary>
    public decimal DefaultAmount { get; set; }
}
