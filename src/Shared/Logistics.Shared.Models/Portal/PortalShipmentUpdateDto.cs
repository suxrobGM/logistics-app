using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>Event and time only; the send outcome stays internal.</summary>
public record PortalShipmentUpdateDto
{
    public CustomerUpdateEvent Event { get; init; }
    public DateTime At { get; init; }
}
