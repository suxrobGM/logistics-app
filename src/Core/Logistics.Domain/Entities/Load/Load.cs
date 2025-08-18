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

    public Guid? TripStopId { get; set; }
    public virtual TripStop? TripStop { get; set; }

    public Guid? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    public Guid? AssignedTruckId { get; set; }
    public virtual Truck? AssignedTruck { get; set; }

    public Guid? AssignedDispatcherId { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }

    public virtual List<LoadInvoice> Invoices { get; set; } = [];
    public virtual List<LoadDocument> Documents { get; set; } = [];

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
    public void Dispatch()
    {
        UpdateStatus(LoadStatus.Dispatched);
    }

    public void ConfirmPickup()
    {
        UpdateStatus(LoadStatus.PickedUp);
    }

    public void ConfirmDelivery()
    {
        UpdateStatus(LoadStatus.Delivered);
    }

    public void Cancel()
    {
        UpdateStatus(LoadStatus.Cancelled);
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
        Customer? customer,
        Truck assignedTruck,
        Employee assignedDispatcher)
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
            AssignedTruckId = assignedTruck.Id,
            AssignedTruck = assignedTruck,
            AssignedDispatcherId = assignedDispatcher.Id,
            AssignedDispatcher = assignedDispatcher,
            CustomerId = customer?.Id,
            Customer = customer
        };

        load.Distance = load.OriginLocation.DistanceTo(load.DestinationLocation);
        var invoice = CreateInvoice(load);
        load.Invoices.Add(invoice);
        load.DomainEvents.Add(new NewLoadCreatedEvent(load.Id));
        return load;
    }

    private static LoadInvoice CreateInvoice(Load load)
    {
        var invoice = new LoadInvoice
        {
            Total = load.DeliveryCost,
            Status = InvoiceStatus.Issued,
            CustomerId = load.CustomerId!.Value,
            Customer = load.Customer!,
            LoadId = load.Id,
            Load = load
        };
        return invoice;
    }
}
