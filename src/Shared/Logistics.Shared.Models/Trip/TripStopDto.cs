using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record TripStopDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public TripStopType Type { get; set; }

    public required Address Address { get; set; }
    public required GeoPoint Location { get; set; }
    public DateTime? ArrivedAt { get; set; }

    public Guid LoadId { get; set; }
    public Guid TripId { get; set; }

    public int GetDemand()
    {
        return Type is TripStopType.PickUp ? 1 : -1;
    }
}
