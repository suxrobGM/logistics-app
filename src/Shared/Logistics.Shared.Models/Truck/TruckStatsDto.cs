namespace Logistics.Shared.Models;

public class TruckStatsDto
{
    public string? TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double Distance { get; set; }
    public decimal Gross { get; set; }
    public decimal DriverShare { get; set; }
    public IEnumerable<EmployeeDto> Drivers { get; set; } = new List<EmployeeDto>();
}
