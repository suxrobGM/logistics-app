using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Encapsulates the state machine logic for trip status transitions.
/// </summary>
public static class TripStatusMachine
{
    private static readonly Dictionary<TripStatus, TripStatus[]> AllowedTransitions = new()
    {
        [TripStatus.Draft] = [TripStatus.Dispatched, TripStatus.Cancelled],
        [TripStatus.Dispatched] = [TripStatus.InTransit, TripStatus.Cancelled],
        [TripStatus.InTransit] = [TripStatus.Completed, TripStatus.Cancelled],
        [TripStatus.Completed] = [],
        [TripStatus.Cancelled] = []
    };

    /// <summary>
    /// Checks if transitioning from the current status to the target status is allowed.
    /// </summary>
    public static bool CanTransition(TripStatus current, TripStatus target)
    {
        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target);
    }

    /// <summary>
    /// Gets all valid next statuses from the current status.
    /// </summary>
    public static TripStatus[] GetAllowedTransitions(TripStatus current)
    {
        return AllowedTransitions.TryGetValue(current, out var allowed) ? allowed : [];
    }

    /// <summary>
    /// Validates that a status transition is allowed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transition is not allowed.</exception>
    public static void ValidateTransition(TripStatus current, TripStatus target, string? customMessage = null)
    {
        if (!CanTransition(current, target))
        {
            throw new InvalidOperationException(
                customMessage ?? $"Cannot change trip status from '{current}' to '{target}'.");
        }
    }

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
