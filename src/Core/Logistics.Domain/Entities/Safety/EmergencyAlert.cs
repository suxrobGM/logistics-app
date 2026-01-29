using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Emergency/SOS alert from driver
/// </summary>
public class EmergencyAlert : AuditableEntity, ITenantEntity
{
    public Guid DriverId { get; set; }
    public virtual Employee Driver { get; set; } = null!;

    public Guid? TruckId { get; set; }
    public virtual Truck? Truck { get; set; }

    public Guid? TripId { get; set; }
    public virtual Trip? Trip { get; set; }

    public required EmergencyAlertType AlertType { get; set; }
    public required EmergencyAlertStatus Status { get; set; }
    public required EmergencyAlertSource Source { get; set; }

    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }

    public string? Description { get; set; }

    // Acknowledgment
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedById { get; set; }
    public virtual Employee? AcknowledgedBy { get; set; }

    // Resolution
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedById { get; set; }
    public virtual Employee? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }

    public virtual List<EmergencyContactNotification> Notifications { get; set; } = [];
}
