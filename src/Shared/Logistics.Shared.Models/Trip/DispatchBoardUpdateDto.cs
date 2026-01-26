using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Models;

/// <summary>
/// DTO for broadcasting dispatch board updates in real-time.
/// </summary>
public record DispatchBoardUpdateDto
{
    /// <summary>
    /// Type of update.
    /// </summary>
    public DispatchBoardUpdateType UpdateType { get; init; }

    /// <summary>
    /// The entity ID that was affected.
    /// </summary>
    public Guid EntityId { get; init; }

    /// <summary>
    /// Additional context data (e.g., trip ID for load assignments).
    /// </summary>
    public Guid? RelatedEntityId { get; init; }

    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Types of dispatch board updates.
/// </summary>
public enum DispatchBoardUpdateType
{
    /// <summary>
    /// A load was assigned to a trip.
    /// </summary>
    [Description("Load Assigned"), EnumMember(Value = "load_assigned")]
    LoadAssigned,

    /// <summary>
    /// A load was unassigned from a trip.
    /// </summary>
    [Description("Load Unassigned"), EnumMember(Value = "load_unassigned")]
    LoadUnassigned,

    /// <summary>
    /// A new trip was created.
    /// </summary>
    [Description("Trip Created"), EnumMember(Value = "trip_created")]
    TripCreated,

    /// <summary>
    /// A trip was dispatched.
    /// </summary>
    [Description("Trip Dispatched"), EnumMember(Value = "trip_dispatched")]
    TripDispatched,

    /// <summary>
    /// A trip was cancelled.
    /// </summary>
    [Description("Trip Cancelled"), EnumMember(Value = "trip_cancelled")]
    TripCancelled,

    /// <summary>
    /// A truck availability changed.
    /// </summary>
    [Description("Truck Availability Changed"), EnumMember(Value = "truck_availability_changed")]
    TruckAvailabilityChanged
}
