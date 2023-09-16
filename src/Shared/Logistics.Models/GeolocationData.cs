namespace Logistics.Models;

public class GeolocationData
{
    public required string UserId { get; set; }
    public required string TenantId { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public string? Address { get; set; }
    public string? UserFullName { get; set; }
    public string? TruckNumber { get; set; }
}
