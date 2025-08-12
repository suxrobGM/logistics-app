namespace Logistics.Shared.Models;

public record CreateTruckCommand
{
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }
}
