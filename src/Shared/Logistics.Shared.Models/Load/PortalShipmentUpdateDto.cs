using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>A shipment update email as the customer sees it: what it said and when.</summary>
public record PortalShipmentUpdateDto
{
    public CustomerUpdateEvent Event { get; init; }
    public DateTime At { get; init; }
}
