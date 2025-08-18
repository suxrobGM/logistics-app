using Logistics.Domain.Core;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Trip : Entity, ITenantEntity
{
    /// <summary>
    ///     Sequential number of the trip, unique within the tenant.
    /// </summary>
    public long Number { get; private set; }

    public required string Name { get; set; }

    /// <summary>
    ///     Total distance of the trip in kilometers.
    /// </summary>
    public double TotalDistance { get; private set; }

    public DateTime PlannedStart { get; set; }
    public DateTime? ActualStart { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public TripStatus Status { get; private set; } = TripStatus.Planned;

    public Guid TruckId { get; set; }
    public virtual required Truck Truck { get; set; }

    public virtual List<TripStop> Stops { get; } = [];


    #region Factory Methods

    public static Trip Create(
        string name,
        DateTime plannedStart,
        Truck truck,
        IEnumerable<Load>? loads = null)
    {
        var loadsArr = loads?.ToArray() ?? [];

        var trip = new Trip
        {
            Name = name,
            TruckId = truck.Id,
            Truck = truck,
            PlannedStart = plannedStart,
            TotalDistance = loadsArr.Sum(l => l.Distance)
        };

        AddStops(trip, loadsArr);

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
        if (Status != TripStatus.Planned)
        {
            throw new InvalidOperationException("Trip already dispatched");
        }

        Status = TripStatus.Dispatched;
        ActualStart = DateTime.UtcNow;
        DomainEvents.Add(new TripDispatchedEvent(Id));
    }

    public void MarkStopArrived(Guid stopId)
    {
        var stop = Stops.FirstOrDefault(s => s.Id == stopId)
                   ?? throw new InvalidOperationException("Stop not found");

        stop.ArrivedAt = DateTime.UtcNow;

        // propagate to Load status
        stop.Load.SetStatus(stop.Type == TripStopType.PickUp ? LoadStatus.PickedUp : LoadStatus.Delivered);

        RefreshStatus();
    }

    private void RefreshStatus()
    {
        if (Stops.All(s => s is { Type: TripStopType.DropOff, Load.DeliveryDate: not null }))
        {
            Status = TripStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            DomainEvents.Add(new TripCompletedEvent(Id));
        }
        else if (Stops.Any(s => s is { Type: TripStopType.PickUp, Load.PickUpDate: not null }))
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
    ///     Updates the trip loads. Clears existing stops and recreates them based on the new loads.
    /// </summary>
    /// <param name="loads">The new collection of loads to be associated with the trip.</param>
    /// <exception cref="InvalidOperationException">Thrown if the trip status is not 'Planned'.</exception>
    public void UpdateTripLoads(IEnumerable<Load> loads)
    {
        if (Status != TripStatus.Planned)
        {
            throw new InvalidOperationException("Cannot update loads for a trip that is not planned");
        }

        // Clear existing stops
        Stops.Clear();
        var loadsArr = loads.ToArray();

        // Recreate stops based on new loads
        AddStops(this, loadsArr);

        TotalDistance = loadsArr.Sum(l => l.Distance);
    }

    /// <summary>
    ///     Adds stops to the given trip for each provided load, creating both pick-up and drop-off stops.
    /// </summary>
    /// <param name="trip">The trip to which the stops will be added.</param>
    /// <param name="loads">The collection of loads for which stops will be created.</param>
    private static void AddStops(Trip trip, IEnumerable<Load> loads)
    {
        // Stops are created in the order of loads
        var order = 1;
        foreach (var load in loads)
        {
            trip.Stops.Add(new TripStop
            {
                Trip = trip,
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
    /// <exception cref="InvalidOperationException">Thrown if the trip status is not 'Planned'.</exception>
    public void RemoveLoad(Guid loadId)
    {
        if (Status != TripStatus.Planned)
        {
            throw new InvalidOperationException("Cannot modify loads unless trip is Planned.");
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
    ///     Adds a load to the trip.
    /// </summary>
    /// <param name="load">The load to be added.</param>
    /// <exception cref="InvalidOperationException">Thrown if the trip status is not 'Planned'.</exception>
    public void AddLoad(Load load)
    {
        if (Status != TripStatus.Planned)
        {
            throw new InvalidOperationException("Cannot modify loads unless trip is Planned.");
        }

        var order = (Stops.Count == 0 ? 0 : Stops.Max(s => s.Order)) + 1;

        Stops.Add(new TripStop
        {
            Trip = this, Order = order++, Type = TripStopType.PickUp, Address = load.OriginAddress,
            Location = load.OriginLocation, Load = load, LoadId = load.Id
        });
        Stops.Add(new TripStop
        {
            Trip = this, Order = order, Type = TripStopType.DropOff, Address = load.DestinationAddress,
            Location = load.DestinationLocation, Load = load, LoadId = load.Id
        });

        TotalDistance = GetLoads().Sum(l => l.Distance);
    }

    #endregion
}
