using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public class TruckGeolocationDto
{
    public required Guid TruckId { get; set; }
    public required Guid TenantId { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public Address? CurrentAddress { get; set; }
    public string? TruckNumber { get; set; }
    public string? DriversName { get; set; }
}
