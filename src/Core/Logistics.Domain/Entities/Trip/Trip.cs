using Logistics.Domain.Core;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Trip : AuditableEntity, ITenantEntity
{
    /// <summary>
    ///     Sequential number of the trip, unique within the tenant.
    /// </summary>
    public long Number { get; private set; }

    public required string Name { get; set; }

    /// <summary>
    ///     Total distance of the trip in kilometers.
    /// </summary>
    public double TotalDistance { get; set; }

    public DateTime? DispatchedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public TripStatus Status { get; private set; } = TripStatus.Draft;

    public Guid TruckId { get; set; }
    public virtual required Truck Truck { get; set; }

    public virtual List<TripStop> Stops { get; } = [];


    #region Factory Methods

    public static Trip Create(
        string name,
        Truck truck,
        IEnumerable<Load>? loads = null,
        IEnumerable<TripStop>? optimizedStops = null)
    {
        var loadsArr = loads?.ToArray() ?? [];

        var trip = new Trip
        {
            Name = name,
            TruckId = truck.Id,
            Truck = truck,
            TotalDistance = loadsArr.Sum(l => l.Distance)
        };

        if (optimizedStops is not null)
        {
            // Ensure Trip reference is set for each stop
            foreach (var stop in optimizedStops)
            {
                stop.Trip = trip;
                stop.TripId = trip.Id;
                trip.Stops.Add(stop);
            }
        }
        else
        {
            AddStops(trip, loadsArr);
        }

        trip.DomainEvents.Add(new NewTripCreatedEvent(trip.Id));
        return trip;
    }

    #endregion

    #region Domain Behaviors

    /// <summary>
    ///     Gets all unique loads associated with the trip.
    /// </summary>
    public IReadOnlyList<Load> GetLoads()
    {
        return Stops.Select(s => s.Load).Where(i => i is not null).Distinct(new LoadComparer()).ToArray();
    }

    public Address GetOriginAddress()
    {
        return Stops.OrderBy(s => s.Order).First().Address;
    }

    public Address GetDestinationAddress()
    {
        return Stops.OrderBy(s => s.Order).Last().Address;
    }

    public void Dispatch()
    {
        if (Status != TripStatus.Draft)
        {
            throw new InvalidOperationException("Trip already dispatched");
        }

        Status = TripStatus.Dispatched;
        DispatchedAt = DateTime.UtcNow;
        DomainEvents.Add(new TripDispatchedEvent(Id));
    }

    public void Cancel()
    {
        if (Status == TripStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed trip");
        }

        Status = TripStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;

        // Mark all stops as canceled
        foreach (var stop in Stops)
        {
            stop.ArrivedAt = null; // Reset arrival time
            stop.Load.Cancel();
        }
    }

    public void MarkStopArrived(Guid stopId)
    {
        var stop = Stops.FirstOrDefault(s => s.Id == stopId)
                   ?? throw new InvalidOperationException("Stop not found");

        stop.ArrivedAt = DateTime.UtcNow;

        // propagate to Load status
        stop.Load.UpdateStatus(stop.Type == TripStopType.PickUp ? LoadStatus.PickedUp : LoadStatus.Delivered, true);

        RefreshStatus();
    }

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

    public decimal CalcTotalRevenue()
    {
        return Stops.Where(s => s.Type == TripStopType.DropOff)
            .Sum(s => s.Load.DeliveryCost.Amount);
    }

    public decimal CalcDriversShare()
    {
        return Stops.Where(s => s.Type == TripStopType.DropOff)
            .Sum(s => s.Load.CalcDriverShare());
    }

    /// <summary>
    ///     Updates the trip stops based on the order of the provided optimized stops list.
    /// </summary>
    /// <param name="optimizedStops">Optional optimized stops with preserved order.</param>
    /// <exception cref="InvalidOperationException">Thrown if the trip status is not 'Draft'.</exception>
    public void UpdateTripStops(IEnumerable<TripStop> optimizedStops)
    {
        if (Status != TripStatus.Draft)
        {
            throw new InvalidOperationException("Cannot update loads for a trip that is not draft.");
        }

        // Use optimized stops - complete replacement strategy
        var optimizedStopsMap = optimizedStops.ToDictionary(s => s.Id, s => s);
        var stops = Stops.ToList();

        foreach (var stop in stops)
        {
            // Just update the order number for existing stops instead of recreating them
            if (optimizedStopsMap.TryGetValue(stop.Id, out var tripStop))
            {
                stop.Order = tripStop.Order;
            }
        }

        TotalDistance = GetLoads().Sum(l => l.Distance);
    }

    /// <summary>
    ///     Adds stops to the given trip for each provided load, creating both pick-up and drop-off stops.
    /// </summary>
    /// <param name="trip">The trip to which the stops will be added.</param>
    /// <param name="loads">The collection of loads for which stops will be created.</param>
    /// <param name="startingOrder">The starting order number for the stops. Defaults to 1.</param>
    private static void AddStops(Trip trip, IEnumerable<Load> loads, int startingOrder = 1)
    {
        // Stops are created in the order of loads
        var order = startingOrder;
        foreach (var load in loads)
        {
            trip.Stops.Add(new TripStop
            {
                Trip = trip,
                TripId = trip.Id,
                Order = order++,
                Type = TripStopType.PickUp,
                Address = load.OriginAddress,
                Location = load.OriginLocation,
                Load = load,
                LoadId = load.Id
            });

            trip.Stops.Add(new TripStop
            {
                Trip = trip,
                TripId = trip.Id,
                Order = order++,
                Type = TripStopType.DropOff,
                Address = load.DestinationAddress,
                Location = load.DestinationLocation,
                Load = load,
                LoadId = load.Id
            });
        }
    }

    /// <summary>
    ///     Removes the load from the trip.
    /// </summary>
    /// <param name="loadId">The ID of the load to be removed.</param>
    /// <exception cref="InvalidOperationException">Thrown if the trip status is not 'Draft'.</exception>
    public void RemoveLoad(Guid loadId)
    {
        if (Status != TripStatus.Draft)
        {
            throw new InvalidOperationException("Cannot modify loads unless trip is Draft.");
        }

        var toRemove = Stops.Where(s => s.LoadId == loadId).ToList();
        if (toRemove.Count == 0)
        {
            return;
        }

        foreach (var s in toRemove)
        {
            Stops.Remove(s);
        }

        // Re-number orders
        var order = 1;
        foreach (var s in Stops.OrderBy(x => x.Order))
        {
            s.Order = order++;
        }

        TotalDistance = GetLoads().Sum(l => l.Distance);
    }

    /// <summary>
    ///     Adds new loads to the trip, creating corresponding pick-up and drop-off stops.
    /// </summary>
    /// <param name="loads">The collection of loads to be added.</param>
    /// <exception cref="InvalidOperationException">Thrown if the trip status is not 'Draft'.</exception>
    public void AddLoads(IEnumerable<Load> loads)
    {
        if (Status != TripStatus.Draft)
        {
            throw new InvalidOperationException("Cannot modify loads unless trip is Draft.");
        }

        var order = (Stops.Count == 0 ? 1 : Stops.Max(s => s.Order)) + 1;

        AddStops(this, loads, order);
        TotalDistance = GetLoads().Sum(l => l.Distance);
    }

    #endregion
}
