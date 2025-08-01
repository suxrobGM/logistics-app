using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record TripStopDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public TripStopType Type { get; set; }

    public required AddressDto Address { get; set; }
    public double? AddressLong { get; set; }
    public double? AddressLat { get; set; }
    
    public DateTime? Planned { get; set; }
    public DateTime? ArrivedAt { get; set; }
    
    public Guid LoadId { get; set; }
}
