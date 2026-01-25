using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record InvoiceLineItemDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public required string Description { get; set; }
    public InvoiceLineItemType Type { get; set; }
    public Money Amount { get; set; } = null!;
    public int Quantity { get; set; }
    public int Order { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Calculated total (Amount * Quantity).
    /// </summary>
    public decimal Total { get; set; }
}
