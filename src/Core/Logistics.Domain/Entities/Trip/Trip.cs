using Logistics.Domain.Core;
using Logistics.Domain.Events;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class Trip : Entity, ITenantEntity
{
    /// <summary>
    /// Sequential number of the trip, unique within the tenant.
    /// </summary>
    public long Number { get; private set; }
    public required string Name { get; set; }

    public Address OriginAddress => Stops.OrderBy(s => s.Order).First().Address;
    public Address DestinationAddress => Stops.OrderBy(s => s.Order).Last().Address;
    
    /// <summary>
    /// Total distance of the trip in kilometers.
    /// </summary>
    public double TotalDistance { get; private set; }

    public DateTime PlannedStart { get; set; }
    public DateTime? ActualStart { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public TripStatus Status { get; private set; } = TripStatus.Planned;

    public Guid TruckId { get; set; }
    public virtual required Truck Truck { get; set; }
    
    public virtual List<TripStop> Stops { get; } = [];
    
    
    #region Domain Behaviors
    
    public void Dispatch()
    {
        if (Status != TripStatus.Planned)
            throw new InvalidOperationException("Trip already dispatched");

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
    
    public decimal CalcTotalRevenue() =>
        Stops.Where(s => s.Type == TripStopType.DropOff)
            .Sum(s => s.Load.DeliveryCost.Amount);

    public decimal CalcDriversShare() =>
        Stops.Where(s => s.Type == TripStopType.DropOff)
            .Sum(s => s.Load.CalcDriverShare());
    
    #endregion


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

        var order = 1;
        
        // Stops are created in the order of loads
        foreach (var load in loadsArr)
        {
            trip.Stops.Add(new TripStop
            {
                Trip = trip,
                Order = order++,
                Type = TripStopType.PickUp,
                Address = load.OriginAddress,
                Load = load,
                LoadId = load.Id
            });

            trip.Stops.Add(new TripStop
            {
                Trip = trip,
                Order = order++,
                Type = TripStopType.DropOff,
                Address = load.DestinationAddress,
                Load = load,
                LoadId = load.Id
            });
        }

        trip.DomainEvents.Add(new NewTripCreatedEvent(trip.Id));
        return trip;
    }

    #endregion
}
