using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Factory methods for Trip entity.
/// </summary>
public partial class Trip
{
    /// <summary>
    /// Creates a new Trip with stops for the provided loads, or with <paramref name="optimizedStops" /> when the
    /// route optimizer already ordered them. Raises NewTripCreatedEvent, and TripAssignedToTruckEvent when a truck
    /// is set.
    /// </summary>
    public static Trip Create(
        string name,
        Truck? truck,
        IEnumerable<Load>? loads = null,
        IEnumerable<TripStop>? optimizedStops = null,
        double? optimizedTotalDistance = null)
    {
        var loadsArr = loads?.ToArray() ?? [];

        var trip = new Trip
        {
            Name = name,
            TruckId = truck?.Id,
            Truck = truck,
            TrailerId = truck?.CurrentTrailerId,
            TotalDistance = optimizedTotalDistance ?? loadsArr.Sum(l => l.Distance)
        };

        if (optimizedStops is not null)
        {
            foreach (var stop in optimizedStops)
            {
                stop.Trip = trip;
                stop.TripId = trip.Id;
                trip.Stops.Add(stop);
            }
        }
        else
        {
            trip.AddStopsForLoads(loadsArr);
        }

        trip.DomainEvents.Add(new NewTripCreatedEvent(trip.Id));

        if (truck is not null)
        {
            trip.RaiseTruckAssignedEvent(truck, oldTruckId: null, oldDriverDeviceToken: null);
        }

        return trip;
    }

    /// <summary>The pickup and drop-off stops for <paramref name="load" />, numbered from <paramref name="order" />.</summary>
    internal static TripStop[] CreateStopPair(Trip trip, Load load, int order) =>
    [
        new TripStop
        {
            Trip = trip,
            TripId = trip.Id,
            Order = order,
            Type = TripStopType.PickUp,
            Address = load.OriginAddress,
            Location = load.OriginLocation,
            Load = load,
            LoadId = load.Id
        },
        new TripStop
        {
            Trip = trip,
            TripId = trip.Id,
            Order = order + 1,
            Type = TripStopType.DropOff,
            Address = load.DestinationAddress,
            Location = load.DestinationLocation,
            Load = load,
            LoadId = load.Id
        }
    ];

    internal int GetNextStopOrder() => Stops.Count == 0 ? 1 : Stops.Max(s => s.Order) + 1;

    private void AddStopsForLoads(IEnumerable<Load> loads)
    {
        var order = GetNextStopOrder();

        foreach (var load in loads)
        {
            Stops.AddRange(CreateStopPair(this, load, order));
            order += 2;
        }
    }

    private void RaiseTruckAssignedEvent(Truck newTruck, Guid? oldTruckId, string? oldDriverDeviceToken)
    {
        DomainEvents.Add(new TripAssignedToTruckEvent(
            Id,
            Name,
            newTruck.Id,
            newTruck.Number,
            newTruck.MainDriver?.DeviceToken,
            newTruck.MainDriver?.GetFullName() ?? newTruck.Number,
            oldTruckId,
            oldDriverDeviceToken));
    }
}
