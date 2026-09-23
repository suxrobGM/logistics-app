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
            stop.DepartedAt = null;
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
        var stop = GetStop(stopId);

        stop.ArrivedAt = DateTime.UtcNow;

        var loadStatus = stop.Type == TripStopType.PickUp
            ? LoadStatus.PickedUp
            : LoadStatus.Delivered;

        stop.Load.UpdateStatus(loadStatus, force: true);

        RefreshStatus();
    }

    /// <summary>
    /// Marks an arrived stop as departed. A repeat keeps the first departure time.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the stop is not found or not arrived yet.</exception>
    public void MarkStopDeparted(Guid stopId)
    {
        var stop = GetStop(stopId);

        if (stop.ArrivedAt is null)
        {
            throw new InvalidOperationException("Mark the stop as arrived before departing.");
        }

        stop.DepartedAt ??= DateTime.UtcNow;
    }

    private TripStop GetStop(Guid stopId)
    {
        return Stops.FirstOrDefault(s => s.Id == stopId)
               ?? throw new InvalidOperationException("Stop not found");
    }

    /// <summary>
    /// Moves the trip to InTransit once any load is picked up, and to Completed once every drop-off is delivered.
    /// A completed or cancelled trip keeps its status.
    /// </summary>
    public void RefreshStatus()
    {
        if (Status is TripStatus.Completed or TripStatus.Cancelled)
        {
            return;
        }

        var allDropOffsDelivered = Stops
            .Where(s => s.Type == TripStopType.DropOff)
            .All(s => s.Load.Status == LoadStatus.Delivered);

        // A delivered load was picked up first, so it also counts.
        var anyPickupDone = Stops
            .Any(s => s is { Type: TripStopType.PickUp, Load.Status: LoadStatus.PickedUp or LoadStatus.Delivered });

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
