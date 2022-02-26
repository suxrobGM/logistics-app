using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Cargo : Entity
{
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public decimal PricePerMile { get; set; }
    public double TotalTripMiles { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? PickUpDate { get; set; }
    public CargoStatus Status { get; set; }
    public string? AssignedDispatcherId { get; set; }

    public virtual Truck? AssignedTruck { get; set; }
    public virtual User? AssignedDispatcher { get; set; }

    public static CargoStatus GetCargoStatus(string value)
    {
        value = value.Trim().ToLower();
        return value switch
        {
            "ready" => CargoStatus.Ready,
            "loaded" => CargoStatus.Loaded,
            "offduty" => CargoStatus.OffDuty,
            _ => CargoStatus.Ready,
        };
    }
}