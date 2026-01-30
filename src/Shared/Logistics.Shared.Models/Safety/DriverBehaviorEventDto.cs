using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Shared.Models;

public class DriverBehaviorEventDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public Guid? TruckId { get; set; }
    public string? TruckNumber { get; set; }

    public DriverBehaviorEventType EventType { get; set; }
    public string EventTypeDisplay { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public EldProviderType ProviderType { get; set; }
    public string ProviderTypeDisplay { get; set; } = string.Empty;

    // Location data
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Location { get; set; }

    // Event details
    public double? SpeedMph { get; set; }
    public double? SpeedLimitMph { get; set; }
    public double? GForce { get; set; }
    public int? DurationSeconds { get; set; }

    // Review workflow
    public bool IsReviewed { get; set; }
    public Guid? ReviewedById { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public bool? IsDismissed { get; set; }
}
