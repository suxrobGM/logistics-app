using Logistics.Domain.Core;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Load : AuditableEntity, ITenantEntity
{
    private static readonly Dictionary<LoadStatus, LoadStatus[]> Allowed = new()
    {
        [LoadStatus.Draft] = [LoadStatus.Dispatched, LoadStatus.Cancelled],
        [LoadStatus.Dispatched] = [LoadStatus.PickedUp, LoadStatus.Cancelled],
        [LoadStatus.PickedUp] = [LoadStatus.Delivered, LoadStatus.Cancelled],
        [LoadStatus.Delivered] = [],
        [LoadStatus.Cancelled] = []
    };

    public long Number { get; private set; }
    public required string Name { get; set; }

    public required LoadType Type { get; set; }

    public LoadStatus Status { get; private set; } = LoadStatus.Draft;

    public required Address OriginAddress { get; set; }
    public required GeoPoint OriginLocation { get; set; }

    public required Address DestinationAddress { get; set; }
    public required GeoPoint DestinationLocation { get; set; }

    public required Money DeliveryCost { get; set; }

    /// <summary>
    ///     Total distance of the load in kilometers.
    /// </summary>
    public double Distance { get; set; }

    public bool CanConfirmPickUp { get; set; }
    public bool CanConfirmDelivery { get; set; }

    public DateTime? DispatchedAt { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public Guid CustomerId { get; set; }
    public virtual required Customer Customer { get; set; }

    public Guid? AssignedTruckId { get; set; }
    public virtual Truck? AssignedTruck { get; set; }

    public Guid? AssignedDispatcherId { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }

    public virtual List<LoadInvoice> Invoices { get; set; } = [];
    public virtual List<LoadDocument> Documents { get; set; } = [];
    public virtual ICollection<TripStop> TripStops { get; } = [];

    /// <summary>
    /// If the load was booked from a load board, the provider type
    /// </summary>
    public LoadBoardProviderType? ExternalSourceProvider { get; set; }

    /// <summary>
    /// External listing ID from the load board provider
    /// </summary>
    public string? ExternalSourceId { get; set; }

    /// <summary>
    /// Broker reference number from the load board
    /// </summary>
    public string? ExternalBrokerReference { get; set; }

    public bool CanTransitionTo(LoadStatus next)
    {
        return Allowed.TryGetValue(Status, out var nexts) && nexts.Contains(next);
    }

    /// <summary>
    ///     Enforces valid status transitions and sets timestamps/flags accordingly.
    /// </summary>
    /// <param name="newStatus">Target status.</param>
    /// <param name="force">
    ///     If true, bypass transition validation (useful for data import/seeding). Still applies timestamp/flag logic.
    /// </param>
    /// <exception cref="InvalidOperationException">Thrown for invalid transitions when <paramref name="force" /> is false.</exception>
    public void UpdateStatus(LoadStatus newStatus, bool force = false)
    {
        if (newStatus == Status)
        {
            return;
        }

        if (!force && !CanTransitionTo(newStatus))
        {
            throw new InvalidOperationException($"Cannot change load status from '{Status}' to '{newStatus}'.");
        }

        switch (newStatus)
        {
            case LoadStatus.Draft:
                // Typically should not revert to Draft; only allowed when force == true.
                CanConfirmPickUp = false;
                CanConfirmDelivery = false;
                break;

            case LoadStatus.Dispatched:
                DispatchedAt ??= DateTime.UtcNow;
                CanConfirmPickUp = true;
                CanConfirmDelivery = false;
                // When (re)dispatching, future milestones are unset
                PickedUpAt = null;
                DeliveredAt = null;
                CancelledAt = null;
                break;

            case LoadStatus.PickedUp:
                PickedUpAt ??= DateTime.UtcNow;
                CanConfirmPickUp = false;
                CanConfirmDelivery = true;
                break;

            case LoadStatus.Delivered:
                DeliveredAt ??= DateTime.UtcNow;
                CanConfirmPickUp = false;
                CanConfirmDelivery = false;
                break;

            case LoadStatus.Cancelled:
                CancelledAt ??= DateTime.UtcNow;
                CanConfirmPickUp = false;
                CanConfirmDelivery = false;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, null);
        }

        Status = newStatus;
    }

    // Convenience wrappers for common actions
    public void Dispatch(DateTime? at = null)
    {
        if (at.HasValue)
        {
            DispatchedAt = at.Value;
        }
        UpdateStatus(LoadStatus.Dispatched);
    }

    public void ConfirmPickup(DateTime? at = null)
    {
        if (at.HasValue)
        {
            PickedUpAt = at.Value;
        }
        UpdateStatus(LoadStatus.PickedUp);
    }

    public void ConfirmDelivery(DateTime? at = null)
    {
        if (at.HasValue)
        {
            DeliveredAt = at.Value;
        }
        UpdateStatus(LoadStatus.Delivered);
    }

    public void Cancel()
    {
        UpdateStatus(LoadStatus.Cancelled);
    }

    /// <summary>
    /// Assigns this load to a truck and raises the LoadAssignedToTruckEvent for notifications.
    /// </summary>
    /// <param name="truck">The truck to assign this load to.</param>
    public void AssignToTruck(Truck truck)
    {
        var oldTruck = AssignedTruck;

        AssignedTruckId = truck.Id;
        AssignedTruck = truck;

        // Raise removal event for old truck (if reassigning)
        if (oldTruck is not null && oldTruck.Id != truck.Id)
        {
            DomainEvents.Add(new LoadRemovedFromTruckEvent(
                Id,
                Number,
                oldTruck.Id,
                oldTruck.Number,
                oldTruck.MainDriver?.DeviceToken,
                oldTruck.SecondaryDriver?.DeviceToken));
        }

        // Raise assignment event for new truck
        var driverDisplayName = truck.MainDriver?.GetFullName() ?? truck.Number;
        DomainEvents.Add(new LoadAssignedToTruckEvent(
            Id,
            Number,
            truck.Id,
            truck.Number,
            truck.MainDriver?.DeviceToken,
            truck.SecondaryDriver?.DeviceToken,
            driverDisplayName));
    }

    /// <summary>
    /// Marks this load as removed from the truck and raises the LoadRemovedFromTruckEvent for notifications.
    /// </summary>
    public void MarkRemovedFromTruck()
    {
        if (AssignedTruck is null)
        {
            return;
        }

        DomainEvents.Add(new LoadRemovedFromTruckEvent(
            Id,
            Number,
            AssignedTruck.Id,
            AssignedTruck.Number,
            AssignedTruck.MainDriver?.DeviceToken,
            AssignedTruck.SecondaryDriver?.DeviceToken));
    }

    /// <summary>
    /// Marks this load as updated and raises the LoadUpdatedEvent for notifications.
    /// </summary>
    public void MarkUpdated()
    {
        if (AssignedTruck is null)
        {
            return;
        }

        DomainEvents.Add(new LoadUpdatedEvent(
            Id,
            Number,
            AssignedTruck.Id,
            AssignedTruck.Number,
            AssignedTruck.MainDriver?.DeviceToken,
            AssignedTruck.SecondaryDriver?.DeviceToken));
    }

    /// <summary>
    /// Updates the proximity status and raises the LoadProximityChangedEvent for notifications.
    /// </summary>
    /// <param name="canConfirmPickUp">Whether the driver can confirm pickup.</param>
    /// <param name="canConfirmDelivery">Whether the driver can confirm delivery.</param>
    public void UpdateProximity(bool? canConfirmPickUp, bool? canConfirmDelivery)
    {
        LoadStatus? statusToConfirm = null;

        if (canConfirmPickUp.HasValue && canConfirmPickUp != CanConfirmPickUp)
        {
            CanConfirmPickUp = canConfirmPickUp.Value;
            if (canConfirmPickUp.Value)
            {
                statusToConfirm = LoadStatus.PickedUp;
            }
        }

        if (canConfirmDelivery.HasValue && canConfirmDelivery != CanConfirmDelivery)
        {
            CanConfirmDelivery = canConfirmDelivery.Value;
            if (canConfirmDelivery.Value)
            {
                statusToConfirm = LoadStatus.Delivered;
            }
        }

        // Raise event if status confirmation changed and truck is assigned
        if (statusToConfirm.HasValue && AssignedTruck is not null)
        {
            DomainEvents.Add(new LoadProximityChangedEvent(
                Id,
                Number,
                statusToConfirm.Value,
                AssignedTruck.Id,
                AssignedTruck.Number,
                AssignedTruck.MainDriver?.DeviceToken,
                AssignedTruck.SecondaryDriver?.DeviceToken));
        }
    }

    public decimal CalcDriverShare()
    {
        return DeliveryCost * (decimal)(AssignedTruck?.GetDriversShareRatio() ?? 0);
    }

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

        // Create trip stops directly in the load entity to avoid EF Core Concurrency issues
        if (trip is not null)
        {
            var tripStops = CreateTripStops(trip, load);
            load.TripStops.Add(tripStops[0]); // pick up stop
            load.TripStops.Add(tripStops[1]); // drop off stop
        }

        load.Distance = load.OriginLocation.DistanceTo(load.DestinationLocation);
        var invoice = CreateInvoice(load);
        load.Invoices.Add(invoice);
        load.DomainEvents.Add(new NewLoadCreatedEvent(load.Id));

        // Raise notification event if truck is assigned at creation
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

    private static LoadInvoice CreateInvoice(Load load)
    {
        var invoice = new LoadInvoice
        {
            Total = load.DeliveryCost,
            Status = InvoiceStatus.Issued,
            CustomerId = load.CustomerId,
            Customer = load.Customer,
            LoadId = load.Id,
            Load = load
        };
        return invoice;
    }

    /// <summary>
    ///     Create trip stops in the linear order (pick up, drop off)
    /// </summary>
    /// <param name="trip">The trip to which the stops will be added.</param>
    /// <param name="load">The load to be added.</param>
    /// <returns>An array of two trip stops.</returns>
    private static TripStop[] CreateTripStops(Trip trip, Load load)
    {
        // Stops are created in the order of loads
        var startingOrder = trip.Stops.Count == 0 ? 1 : trip.Stops.Max(s => s.Order) + 1;
        var tripStops = new TripStop[2];

        tripStops[0] = new TripStop
        {
            Trip = trip,
            TripId = trip.Id,
            Order = startingOrder++,
            Type = TripStopType.PickUp,
            Address = load.OriginAddress,
            Location = load.OriginLocation,
            Load = load,
            LoadId = load.Id
        };

        tripStops[1] = new TripStop
        {
            Trip = trip,
            TripId = trip.Id,
            Order = startingOrder,
            Type = TripStopType.DropOff,
            Address = load.DestinationAddress,
            Location = load.DestinationLocation,
            Load = load,
            LoadId = load.Id
        };

        return tripStops;
    }
}
