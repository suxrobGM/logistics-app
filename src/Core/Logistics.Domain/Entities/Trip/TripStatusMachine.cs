using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Encapsulates the state machine logic for trip status transitions.
/// </summary>
public static class TripStatusMachine
{
    /// <summary>
    /// Checks if the trip can be modified (loads added/removed, truck changed).
    /// Allows modification in Draft and Dispatched statuses.
    /// </summary>
    public static bool CanModify(TripStatus status) =>
        status == TripStatus.Draft || status == TripStatus.Dispatched;

    /// <summary>
    /// Checks if the trip can be cancelled.
    /// </summary>
    public static bool CanCancel(TripStatus status) => status != TripStatus.Completed;

    /// <summary>
    /// Checks if the trip can be dispatched.
    /// </summary>
    public static bool CanDispatch(TripStatus status) => status == TripStatus.Draft;
}
