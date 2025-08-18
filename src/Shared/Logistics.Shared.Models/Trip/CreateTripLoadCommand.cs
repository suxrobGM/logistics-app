using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record CreateTripLoadCommand
{
    public required string Name { get; set; }

    public required Address OriginAddress { get; set; }
    public required GeoPoint OriginLocation { get; set; }

    public required Address DestinationAddress { get; set; }
    public required GeoPoint DestinationLocation { get; set; }

    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public LoadType Type { get; set; }
    public Guid AssignedDispatcherId { get; set; }
    public Guid? CustomerId { get; set; }
}
