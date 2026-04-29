using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Status transition methods for Container entity.
/// </summary>
public partial class Container
{
    public bool CanTransitionTo(ContainerStatus next)
    {
        return ContainerStatusMachine.CanTransition(Status, next);
    }

    /// <summary>
    /// Enforces valid status transitions and updates timestamps accordingly.
    /// Raises <see cref="ContainerStatusChangedEvent"/> on every successful transition.
    /// </summary>
    public void UpdateStatus(ContainerStatus newStatus, bool force = false)
    {
        var previous = Status;

        var timestamps = new ContainerStatusTimestamps(LoadedAt, DeliveredAt, ReturnedAt);
        var result = ContainerStatusMachine.Apply(Status, newStatus, force, timestamps);

        if (result.Status == previous)
        {
            return;
        }

        Status = result.Status;
        LoadedAt = result.Timestamps.LoadedAt;
        DeliveredAt = result.Timestamps.DeliveredAt;
        ReturnedAt = result.Timestamps.ReturnedAt;

        DomainEvents.Add(new ContainerStatusChangedEvent(Id, Number, previous, Status));
    }

    /// <summary>
    /// Marks the container as loaded with cargo. Sets <see cref="IsLaden"/> to true.
    /// </summary>
    public void MarkAsLoaded()
    {
        IsLaden = true;
        UpdateStatus(ContainerStatus.Loaded);
    }

    /// <summary>
    /// Marks the container as empty (cargo discharged or container reset).
    /// Sets <see cref="IsLaden"/> to false.
    /// </summary>
    public void MarkAsEmpty()
    {
        IsLaden = false;
        UpdateStatus(ContainerStatus.Empty);
    }

    /// <summary>
    /// Records that the container is sitting at a terminal.
    /// </summary>
    public void MarkAtPort(Terminal terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        CurrentTerminalId = terminal.Id;
        CurrentTerminal = terminal;
        UpdateStatus(ContainerStatus.AtPort);
    }

    /// <summary>
    /// Records that the container is moving between locations.
    /// </summary>
    public void MarkInTransit()
    {
        UpdateStatus(ContainerStatus.InTransit);
    }

    /// <summary>
    /// Records that the container has reached its delivery point.
    /// </summary>
    public void MarkDelivered()
    {
        UpdateStatus(ContainerStatus.Delivered);
    }

    /// <summary>
    /// Records that the container has been returned to a depot.
    /// </summary>
    public void MarkReturned(Terminal depot)
    {
        ArgumentNullException.ThrowIfNull(depot);
        CurrentTerminalId = depot.Id;
        CurrentTerminal = depot;
        UpdateStatus(ContainerStatus.Returned);
    }

    /// <summary>
    /// Pure location update — moves the container to a terminal without
    /// changing its lifecycle status.
    /// </summary>
    public void MoveToTerminal(Terminal terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        CurrentTerminalId = terminal.Id;
        CurrentTerminal = terminal;
    }
}
