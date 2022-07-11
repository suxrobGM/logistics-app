using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Load : Entity, ITenantEntity
{
    public string? Name { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public decimal PricePerMile { get; set; }
    public double TotalTripMiles { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime PickUpDate { get; set; } = DateTime.Now;
    public LoadStatus Status { get; set; } = LoadStatus.Ready;
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedTruckId { get; set; }

    public virtual Truck? AssignedTruck { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }
}