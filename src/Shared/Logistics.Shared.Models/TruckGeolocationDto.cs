namespace Logistics.Shared.Models;

public class TruckGeolocationDto
{
    public required string TruckId { get; set; }
    public required string TenantId { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public string? CurrentAddress { get; set; }
    public string? TruckNumber { get; set; }
    public string? DriversName { get; set; }
}
