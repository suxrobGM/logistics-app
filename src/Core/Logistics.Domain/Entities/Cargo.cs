using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Cargo : Entity, ITenantEntity
{
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public decimal PricePerMile { get; set; }
    public double TotalTripMiles { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime PickUpDate { get; set; } = DateTime.Now;
    public CargoStatus Status { get; set; } = CargoStatus.Ready;
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedTruckId { get; set; }

    public virtual Truck? AssignedTruck { get; set; }
    public virtual Employee? AssignedDispatcher { get; set; }
}