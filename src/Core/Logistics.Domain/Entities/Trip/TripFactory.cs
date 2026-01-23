using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Factory for creating Trip entities with all required related entities.
/// </summary>
public static class TripFactory
{
    /// <summary>
    /// Creates a new Trip with stops for the provided loads.
    /// Raises NewTripCreatedEvent and TripAssignedToTruckEvent if truck is assigned.
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
            AddStopsForLoads(trip, loadsArr);
        }

        trip.DomainEvents.Add(new NewTripCreatedEvent(trip.Id));

        if (truck is not null)
        {
            RaiseTruckAssignedEvent(trip, truck, oldTruckId: null, oldDriverDeviceToken: null);
        }

        return trip;
    }

    /// <summary>
    /// Adds pickup and drop-off stops for the given loads.
    /// </summary>
    public static void AddStopsForLoads(Trip trip, IEnumerable<Load> loads, int? startingOrder = null)
    {
        var order = startingOrder ?? (trip.Stops.Count == 0 ? 1 : trip.Stops.Max(s => s.Order) + 1);

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

    internal static void RaiseTruckAssignedEvent(
        Trip trip,
        Truck newTruck,
        Guid? oldTruckId,
        string? oldDriverDeviceToken)
    {
        var driverDisplayName = newTruck.MainDriver?.GetFullName() ?? newTruck.Number;

        trip.DomainEvents.Add(new TripAssignedToTruckEvent(
            trip.Id,
            trip.Number,
            trip.Name,
            newTruck.Id,
            newTruck.Number,
            newTruck.MainDriver?.DeviceToken,
            driverDisplayName,
            oldTruckId,
            oldDriverDeviceToken));
    }
}
