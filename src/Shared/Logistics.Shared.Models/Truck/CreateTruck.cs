namespace Logistics.Shared.Models;

public record CreateTruck
{
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }
}
