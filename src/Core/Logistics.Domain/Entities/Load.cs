using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Load : Entity, ITenantEntity
{
    public ulong ReferenceId { get; set; } = 100_000;
    public string? Name { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public decimal DeliveryCost { get; set; }
    public double TotalTripMiles { get; set; }
    public DateTime PickUpDate { get; set; } = DateTime.Now;
    public LoadStatus Status { get; set; } = LoadStatus.Dispatched;
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedTruckId { get; set; }

    public virtual Truck? AssignedTruck { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }
}