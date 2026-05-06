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
    public required Money Amount { get; set; }
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Display order of the line item.
    /// </summary>
    public int Order { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Effective tax rate for this line, populated by <c>ITaxCalculator</c>. 0 when no tax applies
    /// (e.g. reverse-charge B2B export, exempt customer, US tenant without Stripe Tax in
    /// non-registered jurisdiction).
    /// </summary>
    public decimal TaxRatePercent { get; set; }

    /// <summary>
    /// Tax amount in the invoice currency. For inclusive pricing this is the embedded tax;
    /// for exclusive pricing it's the tax added on top.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Stripe Tax product code (txcd_*) or local tax-code identifier. Null falls back to the
    /// tenant's Stripe Tax default.
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// Net (pre-tax) total for this line: Amount * Quantity. Gross is Net + TaxAmount.
    /// </summary>
    public decimal Total => Amount.Amount * Quantity;
}
