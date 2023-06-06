namespace Logistics.Client.Models;

public record Truck
{
    public string? Id { get; set; }
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }
    public string? DriverName { get; set; }
    public IList<string> LoadIds { get; set; } = new List<string>();
}
