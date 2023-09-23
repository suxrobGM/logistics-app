namespace Logistics.Models;

public class TruckStatsDto
{
    public string? TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double Gross { get; set; }
    public double Distance { get; set; }
    public double DriverShare { get; set; }
    public IEnumerable<EmployeeDto> Drivers { get; set; } = new List<EmployeeDto>();
}
