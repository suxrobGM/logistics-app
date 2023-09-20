namespace Logistics.Models;

public class DriverDashboardDto
{
    public string? TruckNumber { get; set; }
    public string? DriverFullName { get; set; }
    public IEnumerable<string> TeammatesName { get; set; } = new List<string>();
    public IEnumerable<LoadDto> ActiveLoads { get; set; } = new List<LoadDto>();
}
