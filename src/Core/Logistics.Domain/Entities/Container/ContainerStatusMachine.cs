using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Encapsulates the state machine logic for container status transitions.
/// </summary>
public static class ContainerStatusMachine
{
    private static readonly Dictionary<ContainerStatus, ContainerStatus[]> AllowedTransitions = new()
    {
        [ContainerStatus.Empty] = [ContainerStatus.Loaded, ContainerStatus.AtPort, ContainerStatus.Returned],
        [ContainerStatus.AtPort] = [ContainerStatus.Loaded, ContainerStatus.InTransit, ContainerStatus.Empty],
        [ContainerStatus.Loaded] = [ContainerStatus.InTransit, ContainerStatus.AtPort],
        [ContainerStatus.InTransit] = [ContainerStatus.Delivered, ContainerStatus.AtPort],
        [ContainerStatus.Delivered] = [ContainerStatus.Empty, ContainerStatus.Returned],
        [ContainerStatus.Returned] = [ContainerStatus.Empty]
    };

    public static bool CanTransition(ContainerStatus current, ContainerStatus target)
    {
        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target);
    }

    public static ContainerStatus[] GetAllowedTransitions(ContainerStatus current)
    {
        return AllowedTransitions.TryGetValue(current, out var allowed) ? allowed : [];
    }

    /// <summary>
    /// Applies the status transition and returns the updated state.
    /// <para>
    /// Leaving <see cref="ContainerStatus.AtPort"/> stamps the outgate (<c>PickedUpAt</c>), which
    /// stops demurrage. Leaving <see cref="ContainerStatus.Returned"/> starts a new move, so the
    /// previous move's timestamps are cleared - otherwise the stale <c>ReturnedAt</c> would read as
    /// "already returned" and silence the next move's per-diem clock.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown for invalid transitions when force is false.</exception>
    public static ContainerStatusState Apply(
        ContainerStatus current,
        ContainerStatus target,
        bool force,
        ContainerStatusTimestamps timestamps,
        DateTime now)
    {
        if (target == current)
        {
            return new ContainerStatusState(current, timestamps);
        }

        if (!force && !CanTransition(current, target))
        {
            throw new InvalidOperationException(
                $"Cannot change container status from '{current}' to '{target}'.");
        }

        if (current == ContainerStatus.Returned)
        {
            timestamps = new ContainerStatusTimestamps(null, null, null, null);
        }
        else if (current == ContainerStatus.AtPort)
        {
            timestamps = timestamps with { PickedUpAt = now };
        }

        return target switch
        {
            ContainerStatus.Loaded => new ContainerStatusState(
                target,
                timestamps with { LoadedAt = timestamps.LoadedAt ?? now }),

            ContainerStatus.Delivered => new ContainerStatusState(
                target,
                timestamps with { DeliveredAt = timestamps.DeliveredAt ?? now }),

            ContainerStatus.Returned => new ContainerStatusState(
                target,
                timestamps with { ReturnedAt = timestamps.ReturnedAt ?? now }),

            ContainerStatus.Empty or ContainerStatus.AtPort or ContainerStatus.InTransit =>
                new ContainerStatusState(target, timestamps),

            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }
}

public record ContainerStatusTimestamps(
    DateTime? LoadedAt,
    DateTime? DeliveredAt,
    DateTime? ReturnedAt,
    DateTime? PickedUpAt);

public record ContainerStatusState(
    ContainerStatus Status,
    ContainerStatusTimestamps Timestamps);
