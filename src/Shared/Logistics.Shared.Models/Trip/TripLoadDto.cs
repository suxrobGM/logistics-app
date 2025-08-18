using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record TripLoadDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public required string Name { get; set; }
    public LoadStatus Status { get; set; }
    public double Distance { get; set; }
    public decimal DeliveryCost { get; set; }
    public required Address OriginAddress { get; set; }
    public required GeoPoint OriginLocation { get; set; }
    public required Address DestinationAddress { get; set; }
    public required GeoPoint DestinationLocation { get; set; }
    public CustomerDto? Customer { get; set; }
}
