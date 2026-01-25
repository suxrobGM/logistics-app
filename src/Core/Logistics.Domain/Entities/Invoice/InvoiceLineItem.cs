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
    /// Calculates the total for this line item (Amount * Quantity).
    /// </summary>
    public decimal Total => Amount.Amount * Quantity;
}
