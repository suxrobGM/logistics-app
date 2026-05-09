using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record DataDeletionRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime ScheduledFor { get; set; }
    public DataDeletionStatus Status { get; set; }
    public string? Reason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    /// <summary>True while the request can still be cancelled.</summary>
    public bool IsCancellable { get; set; }
}
