using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Encapsulates the state machine logic for load status transitions.
/// </summary>
public static class LoadStatusMachine
{
    private static readonly Dictionary<LoadStatus, LoadStatus[]> AllowedTransitions = new()
    {
        [LoadStatus.Draft] = [LoadStatus.Dispatched, LoadStatus.Cancelled],
        [LoadStatus.Dispatched] = [LoadStatus.PickedUp, LoadStatus.Cancelled],
        [LoadStatus.PickedUp] = [LoadStatus.Delivered, LoadStatus.Cancelled],
        [LoadStatus.Delivered] = [],
        [LoadStatus.Cancelled] = []
    };

    /// <summary>
    /// Checks if transitioning from the current status to the target status is allowed.
    /// </summary>
    public static bool CanTransition(LoadStatus current, LoadStatus target)
    {
        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target);
    }

    /// <summary>
    /// Gets all valid next statuses from the current status.
    /// </summary>
    public static LoadStatus[] GetAllowedTransitions(LoadStatus current)
    {
        return AllowedTransitions.TryGetValue(current, out var allowed) ? allowed : [];
    }

    /// <summary>
    /// Applies the status transition and returns the updated state.
    /// </summary>
    /// <param name="current">Current load status.</param>
    /// <param name="target">Target status to transition to.</param>
    /// <param name="force">If true, bypass transition validation.</param>
    /// <param name="timestamps">Current timestamp values.</param>
    /// <returns>Updated state with new status, flags, and timestamps.</returns>
    /// <exception cref="InvalidOperationException">Thrown for invalid transitions when force is false.</exception>
    public static LoadStatusState Apply(
        LoadStatus current,
        LoadStatus target,
        bool force,
        LoadStatusTimestamps timestamps)
    {
        if (target == current)
        {
            return new LoadStatusState(
                current,
                timestamps.CanConfirmPickUp,
                timestamps.CanConfirmDelivery,
                timestamps);
        }

        if (!force && !CanTransition(current, target))
        {
            throw new InvalidOperationException($"Cannot change load status from '{current}' to '{target}'.");
        }

        return target switch
        {
            LoadStatus.Draft => new LoadStatusState(
                target,
                CanConfirmPickUp: false,
                CanConfirmDelivery: false,
                timestamps),

            LoadStatus.Dispatched => new LoadStatusState(
                target,
                CanConfirmPickUp: true,
                CanConfirmDelivery: false,
                timestamps with
                {
                    DispatchedAt = timestamps.DispatchedAt ?? DateTime.UtcNow,
                    PickedUpAt = null,
                    DeliveredAt = null,
                    CancelledAt = null
                }),

            LoadStatus.PickedUp => new LoadStatusState(
                target,
                CanConfirmPickUp: false,
                CanConfirmDelivery: true,
                timestamps with { PickedUpAt = timestamps.PickedUpAt ?? DateTime.UtcNow }),

            LoadStatus.Delivered => new LoadStatusState(
                target,
                CanConfirmPickUp: false,
                CanConfirmDelivery: false,
                timestamps with { DeliveredAt = timestamps.DeliveredAt ?? DateTime.UtcNow }),

            LoadStatus.Cancelled => new LoadStatusState(
                target,
                CanConfirmPickUp: false,
                CanConfirmDelivery: false,
                timestamps with { CancelledAt = timestamps.CancelledAt ?? DateTime.UtcNow }),

            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }
}

/// <summary>
/// Represents the timestamps associated with load status changes.
/// </summary>
public record LoadStatusTimestamps(
    DateTime? DispatchedAt,
    DateTime? PickedUpAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    bool CanConfirmPickUp,
    bool CanConfirmDelivery);

/// <summary>
/// Represents the result of a status transition.
/// </summary>
public record LoadStatusState(
    LoadStatus Status,
    bool CanConfirmPickUp,
    bool CanConfirmDelivery,
    LoadStatusTimestamps Timestamps);
