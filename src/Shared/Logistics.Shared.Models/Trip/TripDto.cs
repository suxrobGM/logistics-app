using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record TripDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public required string Name { get; set; }

    public required AddressDto OriginAddress { get; set; }
    public required AddressDto DestinationAddress { get; set; }
    
    /// <summary>
    /// Total distance of the trip in kilometers.
    /// </summary>
    public double TotalDistance { get; set; }

    public DateTime PlannedStart { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TripStatus Status { get; set; }
    public Guid TruckId { get; set; }
    public string? TruckNumber { get; set; }
    
    public IEnumerable<TripLoadDto> Loads { get; set; } = [];
}
