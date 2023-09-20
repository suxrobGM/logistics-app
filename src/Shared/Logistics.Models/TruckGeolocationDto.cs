namespace Logistics.Models;

public class TruckGeolocationDto
{
    public required string TruckId { get; set; }
    public required string TenantId { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public string? CurrentLocation { get; set; }
    public string? TruckNumber { get; set; }
    public string? DriversName { get; set; }
}
