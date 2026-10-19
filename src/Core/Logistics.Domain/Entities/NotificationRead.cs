using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>One user having read one <see cref="Notification"/>; unique per notification and user.</summary>
public class NotificationRead : Entity, ITenantEntity
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}
