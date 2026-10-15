using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// An extra charge on a load. Also the input shape on load create/update: send <see cref="Id"/> to
/// keep an existing row, leave it null to add one; rows not sent are removed.
/// </summary>
public record LoadAccessorialDto
{
    public Guid? Id { get; set; }
    public Guid? AccessorialChargeId { get; set; }

    /// <summary>Set on detention written when the truck left this stop. Read-only.</summary>
    public Guid? TripStopId { get; set; }

    public required string Name { get; set; }
    public InvoiceLineItemType LineItemType { get; set; }
    public AccessorialUnit Unit { get; set; }
    public decimal Rate { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal Total { get; set; }
}
