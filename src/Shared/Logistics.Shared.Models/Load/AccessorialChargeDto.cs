using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record AccessorialChargeDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public InvoiceLineItemType LineItemType { get; set; }
    public AccessorialUnit Unit { get; set; }
    public decimal DefaultAmount { get; set; }
}
