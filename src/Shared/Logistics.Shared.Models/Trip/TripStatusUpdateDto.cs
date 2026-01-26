using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// DTO for broadcasting trip status changes in real-time.
/// </summary>
public record TripStatusUpdateDto
{
    public Guid TripId { get; init; }
    public string TripName { get; init; } = string.Empty;
    public TripStatus Status { get; init; }
    public TripStatus? PreviousStatus { get; init; }
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public string? Reason { get; init; }
}
