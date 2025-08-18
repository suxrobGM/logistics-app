using Logistics.Domain.Core;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Load : Entity, ITenantEntity
{
    public long Number { get; private set; }
    public required string Name { get; set; }

    public required LoadType Type { get; set; }

    public LoadStatus Status { get; set; } = LoadStatus.Dispatched;

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

    public DateTime DispatchedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

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

    public void SetStatus(LoadStatus status)
    {
        switch (status)
        {
            case LoadStatus.Dispatched:
                DispatchedDate = DateTime.UtcNow;
                CanConfirmDelivery = false;
                CanConfirmPickUp = false;
                PickUpDate = null;
                DeliveryDate = null;
                break;
            case LoadStatus.PickedUp:
                PickUpDate = DateTime.UtcNow;
                CanConfirmDelivery = false;
                DeliveryDate = null;
                break;
            case LoadStatus.Delivered:
                DeliveryDate = DateTime.UtcNow;
                CanConfirmDelivery = false;
                CanConfirmPickUp = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        Status = status;
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

internal class LoadComparer : IEqualityComparer<Load>
{
    public bool Equals(Load? x, Load? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Number == y.Number && x.Name == y.Name;
    }

    public int GetHashCode(Load? obj)
    {
        return HashCode.Combine(obj?.Number, obj?.Name);
    }
}
