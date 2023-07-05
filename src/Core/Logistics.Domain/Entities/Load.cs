using Logistics.Domain.Events;

namespace Logistics.Domain.Entities;

public class Load : AuditableEntity, ITenantEntity
{
    private LoadStatus _status = LoadStatus.Dispatched;

    public ulong RefId { get; set; } = 100_000;
    
    [StringLength(LoadConsts.NameLength)]
    public string? Name { get; set; }
    
    [StringLength(LoadConsts.AddressLength)]
    public string? SourceAddress { get; set; }
    
    [StringLength(LoadConsts.AddressLength)]
    public string? DestinationAddress { get; set; }
    
    [Range(LoadConsts.MinDeliveryCost, LoadConsts.MaxDeliveryCost)]
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
    public string? AssignedDriverId { get; set; }
    public string? AssignedTruckId { get; set; }

    public virtual Truck? AssignedTruck { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }
    public virtual Employee? AssignedDriver { get; set; }

    public static Load CreateLoad(
        ulong refId, string sourceAddress, string destinationAddress,
        Truck assignedTruck, Employee assignedDispatcher)
    {
        var load = new Load
        {
            RefId = refId,
            SourceAddress = sourceAddress,
            DestinationAddress = destinationAddress,
            AssignedTruck = assignedTruck,
            AssignedDriver = assignedTruck.Driver,
            AssignedDispatcher = assignedDispatcher,
            Status = LoadStatus.Dispatched
        };
        
        load.DomainEvents.Add(new NewLoadCreatedEvent(refId));
        return load;
    }
}