using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Factory methods for Load entity.
/// </summary>
public partial class Load
{
    /// <summary>Creates a load; raises NewLoadCreatedEvent, plus LoadAssignedToTruckEvent when a truck is given.</summary>
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
        Trip? trip = null)
    {
        var load = new Load
        {
            Name = name,
            Type = type,
            DeliveryCost = deliveryCost,
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
            foreach (var stop in Trip.CreateStopPair(trip, load, trip.GetNextStopOrder()))
            {
                load.TripStops.Add(stop);
            }
        }

        load.Distance = originLocation.DistanceTo(destinationLocation);
        load.Invoice = LoadInvoice.Create(load);

        load.DomainEvents.Add(new NewLoadCreatedEvent(load.Id));

        if (assignedTruck is not null)
        {
            load.RaiseAssignedToTruckEvent(assignedTruck);
        }

        return load;
    }
}
