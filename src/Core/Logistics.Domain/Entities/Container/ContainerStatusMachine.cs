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
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown for invalid transitions when force is false.</exception>
    public static ContainerStatusState Apply(
        ContainerStatus current,
        ContainerStatus target,
        bool force,
        ContainerStatusTimestamps timestamps)
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

        return target switch
        {
            ContainerStatus.Loaded => new ContainerStatusState(
                target,
                timestamps with { LoadedAt = timestamps.LoadedAt ?? DateTime.UtcNow }),

            ContainerStatus.Delivered => new ContainerStatusState(
                target,
                timestamps with { DeliveredAt = timestamps.DeliveredAt ?? DateTime.UtcNow }),

            ContainerStatus.Returned => new ContainerStatusState(
                target,
                timestamps with { ReturnedAt = timestamps.ReturnedAt ?? DateTime.UtcNow }),

            ContainerStatus.Empty or ContainerStatus.AtPort or ContainerStatus.InTransit =>
                new ContainerStatusState(target, timestamps),

            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }
}

public record ContainerStatusTimestamps(
    DateTime? LoadedAt,
    DateTime? DeliveredAt,
    DateTime? ReturnedAt);

public record ContainerStatusState(
    ContainerStatus Status,
    ContainerStatusTimestamps Timestamps);
