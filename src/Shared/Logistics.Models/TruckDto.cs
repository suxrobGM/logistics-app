namespace Logistics.Models;

public class TruckDto
{
    public string? Id { get; set; }
    public string? TruckNumber { get; set; }
    public string[]? DriverIds { get; set; }
    public string[]? LoadIds { get; set; }
}
