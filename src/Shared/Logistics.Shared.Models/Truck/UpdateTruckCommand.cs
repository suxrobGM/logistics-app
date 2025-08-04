namespace Logistics.Shared.Models;

public record UpdateTruckCommand
{
    public string? Id { get; set; }
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }
}
