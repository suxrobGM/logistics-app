namespace Logistics.Shared.Models;

public class DriverActiveLoadsDto
{
    public string? TruckNumber { get; set; }
    public IEnumerable<LoadDto> ActiveLoads { get; set; } = new List<LoadDto>();
}
