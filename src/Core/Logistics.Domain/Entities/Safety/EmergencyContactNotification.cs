using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Record of notification sent to an emergency contact
/// </summary>
public class EmergencyContactNotification : Entity, ITenantEntity
{
    public Guid EmergencyAlertId { get; set; }
    public virtual EmergencyAlert EmergencyAlert { get; set; } = null!;

    public Guid EmergencyContactId { get; set; }
    public virtual EmergencyContact EmergencyContact { get; set; } = null!;

    public required NotificationMethod Method { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}
