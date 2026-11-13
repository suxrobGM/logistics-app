using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a line item on an invoice.
/// </summary>
public class InvoiceLineItem : Entity, ITenantEntity
{
    public required Guid InvoiceId { get; set; }
    public virtual Invoice Invoice { get; set; } = null!;

    public required string Description { get; set; }
    public required InvoiceLineItemType Type { get; set; }
    public required decimal Amount { get; set; }
    /// <summary>Number of units billed (hours, distance or count); fractional for metered accessorials.</summary>
    public decimal Quantity { get; set; } = 1;

    /// <summary>
    /// Display order of the line item.
    /// </summary>
    public int Position { get; set; }

    public string? Notes { get; set; }

    /// <summary>Id of the record a generated line came from; null for hand-added lines, which rebuilds leave in place. Not a foreign key, since sources span several tables.</summary>
    public Guid? SourceId { get; set; }

    /// <summary>Tax rate in percent; 0 when no tax applies (reverse charge, exempt customer, or a jurisdiction where the tenant is not registered).</summary>
    public decimal TaxRatePercent { get; set; }

    /// <summary>Tax on this line: the tax included in the amount for tax-inclusive pricing, or added on top for tax-exclusive pricing.</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Stripe Tax product code; null uses the tenant's default tax code.</summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// Net (pre-tax) total for this line: Amount * Quantity. Gross is Net + TaxAmount.
    /// </summary>
    public decimal Total => Amount * Quantity;
}
