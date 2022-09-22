using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Load : Entity, ITenantEntity
{
    public Load()
    {
        Status = LoadStatus.Dispatched;
    }
    
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
    
    [Range(LoadConsts.MinDistance, LoadConsts.MaxDistance)]
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
            
            if (_status == LoadStatus.Dispatched)
            {
                DispatchedDate = DateTime.UtcNow;
                PickUpDate = null;
                DeliveryDate = null;
            }
            else if (_status == LoadStatus.PickedUp)
            {
                PickUpDate = DateTime.UtcNow;
                DeliveryDate = null;
            }
            else if (_status == LoadStatus.Delivered)
            {
                DeliveryDate = DateTime.UtcNow;
            }
        }
    }
    
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedDriverId { get; set; }
    public string? AssignedTruckId { get; set; }

    public virtual Truck? AssignedTruck { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }
    public virtual Employee? AssignedDriver { get; set; }
}