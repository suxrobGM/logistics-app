using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Factory methods for Load entity.
/// </summary>
public partial class Load
{
    /// <summary>
    /// Creates a new Load with a draft invoice, and its pickup and drop-off stops when it starts on
    /// <paramref name="trip" />. Raises NewLoadCreatedEvent, and LoadAssignedToTruckEvent when a truck is set.
    /// </summary>
    public static Load Create(
        string name,
        LoadType type,
        decimal deliveryCost,
        Address originAddress,
        GeoPoint originLocation,
        Address destinationAddress,
        GeoPoint destinationLocation,
        Customer customer,
        Truck? assignedTruck,
        Employee assignedDispatcher,
        string currency,
        Trip? trip = null)
    {
        var load = new Load
        {
            Name = name,
            Type = type,
            DeliveryCost = new() { Amount = deliveryCost, Currency = currency },
            OriginAddress = originAddress,
            OriginLocation = originLocation,
            DestinationAddress = destinationAddress,
            DestinationLocation = destinationLocation,
            AssignedTruckId = assignedTruck?.Id,
            AssignedTruck = assignedTruck,
            AssignedDispatcherId = assignedDispatcher.Id,
            AssignedDispatcher = assignedDispatcher,
            CustomerId = customer.Id,
            Customer = customer
        };

        if (trip is not null)
        {
            // Linked through the load only; EF adds them to trip.Stops when it saves the graph.
            var order = trip.Stops.Count == 0 ? 1 : trip.Stops.Max(s => s.Order) + 1;
            foreach (var stop in Trip.CreateStopPair(trip, load, order))
            {
                load.TripStops.Add(stop);
            }
        }

        load.Distance = originLocation.DistanceTo(destinationLocation);
        load.Invoice = LoadInvoice.Create(load);

        load.DomainEvents.Add(new NewLoadCreatedEvent(load.Id));

        if (assignedTruck is not null)
        {
            load.DomainEvents.Add(new LoadAssignedToTruckEvent(
                load.Id,
                assignedTruck.Id,
                assignedTruck.Number,
                assignedTruck.MainDriver?.DeviceToken,
                assignedTruck.SecondaryDriver?.DeviceToken,
                assignedTruck.MainDriver?.GetFullName() ?? assignedTruck.Number));
        }

        return load;
    }
}
