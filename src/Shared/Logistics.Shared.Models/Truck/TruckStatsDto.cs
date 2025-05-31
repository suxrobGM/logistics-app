namespace Logistics.Shared.Models;

public record TruckStatsDto
{
    public Guid? TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double Distance { get; set; }
    public decimal Gross { get; set; }
    public decimal DriverShare { get; set; }
    public List<EmployeeDto> Drivers { get; set; } = [];
    public int TotalItems { get; set; }
}
