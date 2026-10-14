using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

public record LineItemTypeMapping
{
    public required InvoiceLineItemType LineItemType { get; set; }
    public required string QboId { get; set; }
}
