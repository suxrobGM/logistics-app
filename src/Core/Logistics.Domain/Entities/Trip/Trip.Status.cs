using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Status transition methods for Trip entity.
/// </summary>
public partial class Trip
{
    /// <summary>
    /// Dispatches the trip and each of its loads still in Draft, which issues their invoices, and records
    /// the truck's drivers.
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
        RecordDrivers();

        foreach (var load in GetLoads().Where(l => l.Status == LoadStatus.Draft))
        {
            load.Dispatch();
        }

        DomainEvents.Add(new TripDispatchedEvent(Id));
    }

    /// <summary>
    /// Cancels the trip and all associated loads.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the trip is already completed, or any of its loads is delivered: a delivered load can't be
    /// cancelled, so its remaining loads are cancelled one by one instead.
    /// </exception>
    public void Cancel()
    {
        if (!TripStatusMachine.CanCancel(Status))
        {
            throw new InvalidOperationException("Cannot cancel a completed trip");
        }

        if (Stops.Any(s => s.Load.Status == LoadStatus.Delivered))
        {
            throw new InvalidOperationException(
                "This trip has delivered loads. Cancel its remaining loads individually instead.");
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
    /// Marks a stop as arrived, picks up or delivers its load, and updates the trip status accordingly.
    /// A drop-off reached before its pickup picks the load up first; a load already past that point,
    /// or cancelled, keeps its status.
    /// </summary>
    /// <param name="stopId">The ID of the stop to mark as arrived.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the stop is not found or already arrived, or the trip is not Dispatched or InTransit.
    /// </exception>
    public void MarkStopArrived(Guid stopId)
    {
        if (Status is not (TripStatus.Dispatched or TripStatus.InTransit))
        {
            throw new InvalidOperationException($"Stops can't be arrived at on a trip that is {Status}.");
        }

        var stop = GetStop(stopId);

        if (stop.ArrivedAt is not null)
        {
            throw new InvalidOperationException("This stop is already marked as arrived.");
        }

        stop.ArrivedAt = DateTime.UtcNow;

        var load = stop.Load;
        if (load.Status == LoadStatus.Dispatched)
        {
            load.ConfirmPickup();
        }

        if (stop.Type == TripStopType.DropOff && load.Status == LoadStatus.PickedUp)
        {
            load.ConfirmDelivery();
        }

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

    private void RecordDrivers()
    {
        DriverId = Truck?.MainDriverId;
        SecondaryDriverId = Truck?.SecondaryDriverId;
    }

    private TripStop GetStop(Guid stopId)
    {
        return Stops.FirstOrDefault(s => s.Id == stopId)
               ?? throw new InvalidOperationException("Stop not found");
    }

    /// <summary>
    /// Derives the trip's status from its loads, ignoring cancelled ones: the trip is Cancelled once every
    /// load is cancelled, Completed once every other load is delivered, and InTransit once any is picked
    /// up. A draft trip only follows its loads into Cancelled; a completed or cancelled trip keeps its status.
    /// </summary>
    public void RefreshStatus()
    {
        if (Status is TripStatus.Completed or TripStatus.Cancelled)
        {
            return;
        }

        if (Stops.Count == 0)
        {
            return;
        }

        var remaining = Stops.Select(s => s.Load).Where(l => l.Status != LoadStatus.Cancelled).ToArray();

        if (remaining.Length == 0)
        {
            Status = TripStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            return;
        }

        if (Status == TripStatus.Draft)
        {
            return;
        }

        if (remaining.All(l => l.Status == LoadStatus.Delivered))
        {
            Status = TripStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            DomainEvents.Add(new TripCompletedEvent(Id));
        }
        // A delivered load was picked up first, so it also counts.
        else if (remaining.Any(l => l.Status is LoadStatus.PickedUp or LoadStatus.Delivered))
        {
            Status = TripStatus.InTransit;
        }
    }
}
