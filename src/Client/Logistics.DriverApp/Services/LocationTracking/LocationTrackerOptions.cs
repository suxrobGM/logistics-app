namespace Logistics.DriverApp.Services.LocationTracking;

public class LocationTrackerOptions
{
    public Guid? TruckId { get; set; }
    public Guid? TenantId { get; set; }
    public string? TruckNumber { get; set; }
    public string? DriversName { get; set; }
}
