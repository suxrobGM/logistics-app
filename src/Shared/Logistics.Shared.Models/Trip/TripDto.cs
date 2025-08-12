using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record TripDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public required string Name { get; set; }

    public required Address OriginAddress { get; set; }
    public required Address DestinationAddress { get; set; }

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

    public IEnumerable<TripStopDto> Stops { get; set; } = [];

    public IEnumerable<TripLoadDto> Loads { get; set; } = [];
}
