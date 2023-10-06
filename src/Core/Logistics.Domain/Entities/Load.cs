using Logistics.Domain.Constraints;
using Logistics.Domain.Enums;
using Logistics.Domain.Events;

namespace Logistics.Domain.Entities;

public class Load : AuditableEntity, ITenantEntity
{
    public ulong RefId { get; set; } = 1000;
    
    [StringLength(LoadConsts.NameLength)]
    public string? Name { get; set; }
    
    [StringLength(LoadConsts.AddressLength)]
    public string? OriginAddress { get; set; }
    
    public double? OriginAddressLat { get; set; }
    public double? OriginAddressLong { get; set; }
    
    [StringLength(LoadConsts.AddressLength)]
    public string? DestinationAddress { get; set; }
    
    public double? DestinationAddressLat { get; set; }
    public double? DestinationAddressLong { get; set; }
    
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    
    public bool CanConfirmPickUp { get; set; }
    public bool CanConfirmDelivery { get; set; }
    
    public DateTime DispatchedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedTruckId { get; set; }

    public virtual Truck? AssignedTruck { get; set; }
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
        return DeliveryCost * (decimal)(AssignedTruck?.DriverIncomePercentage ?? 0);
    }

    public static Load Create(
        ulong refId, 
        string originAddress,
        double originLatitude,
        double originLongitude,
        string destinationAddress,
        double destinationLatitude,
        double destinationLongitude,
        Truck assignedTruck, 
        Employee assignedDispatcher)
    {
        var load = new Load
        {
            RefId = refId,
            OriginAddress = originAddress,
            OriginAddressLat = originLatitude,
            OriginAddressLong = originLongitude,
            DestinationAddress = destinationAddress,
            DestinationAddressLat = destinationLatitude,
            DestinationAddressLong = destinationLongitude,
            AssignedTruck = assignedTruck,
            AssignedDispatcher = assignedDispatcher
        };
        
        load.DomainEvents.Add(new NewLoadCreatedEvent(refId));
        return load;
    }
}
