namespace Logistics.Shared.Models;

public class TruckDto
{
    public string? Id { get; set; }
    public string? TruckNumber { get; set; }
    public AddressDto? CurrentLocation { get; set; }
    public double? CurrentLocationLat { get; set; }
    public double? CurrentLocationLong { get; set; }
    public IEnumerable<EmployeeDto> Drivers { get; set; } = new List<EmployeeDto>();
    public IEnumerable<LoadDto> Loads { get; set; } = new List<LoadDto>();
}
