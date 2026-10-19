using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// A tenant-wide notification. Whether it is read is per user - see <see cref="NotificationRead"/>.
/// </summary>
public class Notification : Entity, ITenantEntity
{
    public required string Title { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
