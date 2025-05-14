using Logistics.Domain.Core;
using Logistics.Shared.Consts;
using Logistics.Domain.Events;
using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Load : Entity, ITenantEntity
{
    public long Number { get; set; }
    public string? Name { get; set; }
    
    public required Address OriginAddress { get; set; }
    public double? OriginAddressLat { get; set; }
    public double? OriginAddressLong { get; set; }
    
    public required Address DestinationAddress { get; set; }
    public double? DestinationAddressLat { get; set; }
    public double? DestinationAddressLong { get; set; }
    
    public required Money DeliveryCost { get; set; }
    public double Distance { get; set; }
    public bool CanConfirmPickUp { get; set; }
    public bool CanConfirmDelivery { get; set; }
    
    public DateTime DispatchedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    public Guid? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }
    
    public Guid? AssignedTruckId { get; set; }
    public virtual Truck? AssignedTruck { get; set; }
    
    public Guid? AssignedDispatcherId { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }

    public virtual List<LoadInvoice> Invoices { get; set; } = [];

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
    }

    public LoadStatus GetStatus()
    {
        if (DeliveryDate.HasValue)
        {
            return LoadStatus.Delivered;
        }

        return PickUpDate.HasValue ? LoadStatus.PickedUp : LoadStatus.Dispatched;
    }

    public decimal CalcDriverShare()
    {
        return DeliveryCost * (decimal)(AssignedTruck?.GetDriversShareRatio() ?? 0);
    }

    public static Load Create(
        decimal deliveryCost,
        Address originAddress,
        double originLatitude,
        double originLongitude,
        Address destinationAddress,
        double destinationLatitude,
        double destinationLongitude,
        Customer customer,
        Truck assignedTruck, 
        Employee assignedDispatcher)
    {
        var load = new Load
        {
            DeliveryCost = deliveryCost,
            OriginAddress = originAddress,
            OriginAddressLat = originLatitude,
            OriginAddressLong = originLongitude,
            DestinationAddress = destinationAddress,
            DestinationAddressLat = destinationLatitude,
            DestinationAddressLong = destinationLongitude,
            AssignedTruckId = assignedTruck.Id,
            AssignedTruck = assignedTruck,
            AssignedDispatcherId = assignedDispatcher.Id,
            AssignedDispatcher = assignedDispatcher,
            CustomerId = customer.Id,
            Customer = customer
        };

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
            Load = load,
        };
        return invoice;
    }
}
