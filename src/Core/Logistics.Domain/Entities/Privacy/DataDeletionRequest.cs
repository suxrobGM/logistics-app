using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a GDPR Article 17 right-to-erasure request with a 30-day grace period.
/// Stored in the master DB because users span tenants.
/// </summary>
public class DataDeletionRequest : Entity, IMasterEntity
{
    public required Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Earliest moment the deletion job is allowed to anonymize the user
    /// (= RequestedAt + grace period). Cancellable up until this time.
    /// </summary>
    public DateTime ScheduledFor { get; set; }

    public DataDeletionStatus Status { get; set; } = DataDeletionStatus.Pending;

    /// <summary>
    /// Optional free-text reason supplied by the user.
    /// </summary>
    public string? Reason { get; set; }

    public DateTime? CancelledAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
