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
    public User? Driver { get; set; }
    public User? Dispatcher { get; set; }
}