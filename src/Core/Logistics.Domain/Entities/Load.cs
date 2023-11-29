using Logistics.Domain.Core;
using Logistics.Shared.Enums;
using Logistics.Domain.Events;
using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Load : Entity, ITenantEntity
{
    public ulong RefId { get; set; } = 1000;
    public string? Name { get; set; }
    
    public required Address OriginAddress { get; set; }
    public double? OriginAddressLat { get; set; }
    public double? OriginAddressLong { get; set; }
    
    public required Address DestinationAddress { get; set; }
    public double? DestinationAddressLat { get; set; }
    public double? DestinationAddressLong { get; set; }
    
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public bool CanConfirmPickUp { get; set; }
    public bool CanConfirmDelivery { get; set; }
    
    public DateTime DispatchedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    public string? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }
    
    public string? InvoiceId { get; set; }
    public virtual Invoice? Invoice { get; set; }
    
    public string? AssignedTruckId { get; set; }
    public virtual Truck? AssignedTruck { get; set; }
    
    public string? AssignedDispatcherId { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }

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
        ulong refId,
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
            RefId = refId,
            DeliveryCost = deliveryCost,
            OriginAddress = originAddress,
            OriginAddressLat = originLatitude,
            OriginAddressLong = originLongitude,
            DestinationAddress = destinationAddress,
            DestinationAddressLat = destinationLatitude,
            DestinationAddressLong = destinationLongitude,
            AssignedTruck = assignedTruck,
            AssignedDispatcher = assignedDispatcher,
            Customer = customer
        };

        var invoice = CreateInvoice(load);
        load.Invoice = invoice;
        load.InvoiceId = invoice.Id;
        load.DomainEvents.Add(new NewLoadCreatedEvent(refId));
        return load;
    }

    private static Invoice CreateInvoice(Load load)
    {
        var payment = new Payment
        {
            Amount = load.DeliveryCost,
            Status = PaymentStatus.Pending,
            PaymentFor = PaymentFor.Invoice
        };

        var invoice = new Invoice
        {
            CustomerId = load.CustomerId!,
            Customer = load.Customer!,
            LoadId = load.Id,
            Load = load,
            PaymentId = payment.Id,
            Payment = payment
        };

        return invoice;
    }
}
