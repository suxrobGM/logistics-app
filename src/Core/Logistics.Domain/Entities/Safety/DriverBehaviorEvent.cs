using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Driver safety events from ELD providers (harsh braking, speeding, etc.)
/// </summary>
public class DriverBehaviorEvent : Entity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public Guid? TruckId { get; set; }
    public virtual Truck? Truck { get; set; }

    public required DriverBehaviorEventType EventType { get; set; }
    public required DateTime OccurredAt { get; set; }
    public required EldProviderType ProviderType { get; set; }

    // Location data
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Location { get; set; }

    // Event details
    public double? SpeedMph { get; set; }
    public double? SpeedLimitMph { get; set; }
    public double? GForce { get; set; }
    public int? DurationSeconds { get; set; }

    public string? ExternalEventId { get; set; }
    public string? RawEventDataJson { get; set; }

    // Review workflow
    public bool IsReviewed { get; set; }
    public Guid? ReviewedById { get; set; }
    public virtual Employee? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public bool? IsDismissed { get; set; }
}
