using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Factory for creating Load entities with all required related entities.
/// </summary>
public static class LoadFactory
{
    /// <summary>
    /// Creates a new Load with an invoice and optional trip stops.
    /// Raises NewLoadCreatedEvent and LoadAssignedToTruckEvent if truck is assigned.
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
            var tripStops = CreateTripStops(trip, load);
            load.TripStops.Add(tripStops[0]);
            load.TripStops.Add(tripStops[1]);
        }

        load.Distance = originLocation.DistanceTo(destinationLocation);

        var invoice = CreateInvoice(load);
        load.Invoices.Add(invoice);

        load.DomainEvents.Add(new NewLoadCreatedEvent(load.Id));

        if (assignedTruck is not null)
        {
            var driverDisplayName = assignedTruck.MainDriver?.GetFullName() ?? assignedTruck.Number;
            load.DomainEvents.Add(new LoadAssignedToTruckEvent(
                load.Id,
                load.Number,
                assignedTruck.Id,
                assignedTruck.Number,
                assignedTruck.MainDriver?.DeviceToken,
                assignedTruck.SecondaryDriver?.DeviceToken,
                driverDisplayName));
        }

        return load;
    }

    /// <summary>
    /// Creates an invoice for the load.
    /// </summary>
    public static LoadInvoice CreateInvoice(Load load)
    {
        return new LoadInvoice
        {
            Total = load.DeliveryCost,
            Status = InvoiceStatus.Issued,
            CustomerId = load.CustomerId,
            Customer = load.Customer,
            LoadId = load.Id,
            Load = load
        };
    }

    /// <summary>
    /// Creates trip stops in linear order (pick up, drop off).
    /// </summary>
    public static TripStop[] CreateTripStops(Trip trip, Load load)
    {
        var startingOrder = trip.Stops.Count == 0 ? 1 : trip.Stops.Max(s => s.Order) + 1;

        return
        [
            new TripStop
            {
                Trip = trip,
                TripId = trip.Id,
                Order = startingOrder,
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
                Order = startingOrder + 1,
                Type = TripStopType.DropOff,
                Address = load.DestinationAddress,
                Location = load.DestinationLocation,
                Load = load,
                LoadId = load.Id
            }
        ];
    }
}
