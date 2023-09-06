namespace Logistics.Models;

public class TruckDto
{
    public string? Id { get; set; }
    public string? TruckNumber { get; set; }
    public string? DriverId { get; set; }
    public string? DriverName { get; set; }
    public IList<string> LoadIds { get; set; } = new List<string>();
}
