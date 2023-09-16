using Logistics.Domain.Constraints;
using Logistics.Domain.Enums;
using Logistics.Domain.Events;

namespace Logistics.Domain.Entities;

public class Load : AuditableEntity, ITenantEntity
{
    private LoadStatus _status = LoadStatus.Dispatched;

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
    
    public double DeliveryCost { get; set; }
    public double Distance { get; set; }
    
    public DateTime DispatchedDate { get; set; }
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    
    public LoadStatus Status
    {
        get => _status;
        set
        {
            _status = value;

            switch (_status)
            {
                case LoadStatus.Dispatched:
                    DispatchedDate = DateTime.UtcNow;
                    PickUpDate = null;
                    DeliveryDate = null;
                    break;
                case LoadStatus.PickedUp:
                    PickUpDate = DateTime.UtcNow;
                    DeliveryDate = null;
                    break;
                case LoadStatus.Delivered:
                    DeliveryDate = DateTime.UtcNow;
                    break;
            }
        }
    }
    
    public string? AssignedDispatcherId { get; set; }
    // public string? AssignedDriverId { get; set; }
    public string? AssignedTruckId { get; set; }

    public virtual Truck? AssignedTruck { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }
    // public virtual Employee? AssignedDriver { get; set; }

    public static Load CreateLoad(
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
            AssignedDispatcher = assignedDispatcher,
            Status = LoadStatus.Dispatched
        };
        
        load.DomainEvents.Add(new NewLoadCreatedEvent(refId));
        return load;
    }
}
