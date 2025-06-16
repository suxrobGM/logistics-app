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
    public long Number { get; set; }
    public string? Name { get; set; }

    public required Address OriginAddress { get; set; }
    public required Address DestinationAddress { get; set; }
    
    /// <summary>
    /// Total distance of the trip in kilometers.
    /// </summary>
    public double TotalDistance { get; private set; }

    public DateTime PlannedStart { get; set; }
    public DateTime? ActualStart { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public TripStatus Status { get; private set; } = TripStatus.Planned;

    public Guid TruckId { get; set; }
    public virtual Truck Truck { get; set; } = null!;

    /// <summary>
    /// Loads (cars) carried on this trip in stop order.
    /// </summary>
    public virtual List<TripLoad> Loads { get; } = [];
    
    #region Domain Behaviors
    
    public void Dispatch()
    {
        if (Status != TripStatus.Planned)
            throw new InvalidOperationException($"Cannot dispatch trip in state {Status}");

        Status = TripStatus.Dispatched;
        ActualStart = DateTime.UtcNow;
        DomainEvents.Add(new TripDispatchedEvent(Id));
    }

    public void MarkLoadPickedUp(Guid loadId)
    {
        var item = Loads.FirstOrDefault(tl => tl.LoadId == loadId)
                   ?? throw new InvalidOperationException("Load not found in trip");

        item.Load.SetStatus(LoadStatus.PickedUp);
        RefreshStatus();
    }

    public void MarkLoadDelivered(Guid loadId)
    {
        var item = Loads.FirstOrDefault(tl => tl.LoadId == loadId)
                   ?? throw new InvalidOperationException("Load not found in trip");

        item.Load.SetStatus(LoadStatus.Delivered);
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        if (Loads.All(l => l.Load.DeliveryDate.HasValue))
        {
            Status = TripStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            DomainEvents.Add(new TripCompletedEvent(Id));
        }
        else if (Loads.Any(l => l.Load.PickUpDate.HasValue))
        {
            Status = TripStatus.InTransit;
        }
    }

    public decimal CalcTotalRevenue() => Loads.Sum(l => l.Load.DeliveryCost.Amount);

    public decimal CalcDriversShare() => Loads.Sum(l => l.Load.CalcDriverShare());
    
    #endregion


    #region Factory Methods

    public static Trip Create(
        Truck truck,
        Address origin,
        Address destination,
        DateTime plannedStart,
        IEnumerable<(Load load, int stopOrder)> loads,
        long number,
        string? name = null)
    {
        if (!loads.Any())
            throw new ArgumentException("Trip must contain at least one load");

        var trip = new Trip
        {
            Number = number,
            Name = name,
            OriginAddress = origin,
            DestinationAddress = destination,
            TruckId = truck.Id,
            Truck = truck,
            PlannedStart = plannedStart,
            Status = TripStatus.Planned,
            TotalDistance = loads.Sum(l => l.load.Distance)
        };

        foreach (var (load, order) in loads)
        {
            trip.Loads.Add(new TripLoad
            {
                Trip = trip,
                Load = load,
                StopOrder = order
            });
        }

        trip.DomainEvents.Add(new NewTripCreatedEvent(trip.Id));
        return trip;
    }

    #endregion
}