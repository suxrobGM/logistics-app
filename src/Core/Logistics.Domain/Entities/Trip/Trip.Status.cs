using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Status transition methods for Trip entity.
/// </summary>
public partial class Trip
{
    /// <summary>
    /// Dispatches the trip.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if trip is not in Draft status.</exception>
    public void Dispatch()
    {
        if (!TripStatusMachine.CanDispatch(Status))
        {
            throw new InvalidOperationException("Trip already dispatched");
        }

        Status = TripStatus.Dispatched;
        DispatchedAt = DateTime.UtcNow;
        DomainEvents.Add(new TripDispatchedEvent(Id));
    }

    /// <summary>
    /// Cancels the trip and all associated loads.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if trip is already completed.</exception>
    public void Cancel()
    {
        if (!TripStatusMachine.CanCancel(Status))
        {
            throw new InvalidOperationException("Cannot cancel a completed trip");
        }

        Status = TripStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;

        foreach (var stop in Stops)
        {
            stop.ArrivedAt = null;
            stop.Load.Cancel();
        }
    }

    /// <summary>
    /// Marks a stop as arrived and updates the trip status accordingly.
    /// </summary>
    /// <param name="stopId">The ID of the stop to mark as arrived.</param>
    /// <exception cref="InvalidOperationException">Thrown if stop is not found.</exception>
    public void MarkStopArrived(Guid stopId)
    {
        var stop = Stops.FirstOrDefault(s => s.Id == stopId)
                   ?? throw new InvalidOperationException("Stop not found");

        stop.ArrivedAt = DateTime.UtcNow;

        var loadStatus = stop.Type == TripStopType.PickUp
            ? LoadStatus.PickedUp
            : LoadStatus.Delivered;

        stop.Load.UpdateStatus(loadStatus, force: true);

        RefreshStatus();
    }

    /// <summary>
    /// Refreshes the trip status based on the current state of stops.
    /// </summary>
    private void RefreshStatus()
    {
        var allDropOffsDelivered = Stops
            .Where(s => s.Type == TripStopType.DropOff)
            .All(s => s.Load.Status == LoadStatus.Delivered);

        var anyPickupDone = Stops
            .Any(s => s is { Type: TripStopType.PickUp, Load.Status: LoadStatus.PickedUp });

        if (allDropOffsDelivered)
        {
            Status = TripStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            DomainEvents.Add(new TripCompletedEvent(Id));
        }
        else if (anyPickupDone)
        {
            Status = TripStatus.InTransit;
        }
    }
}
