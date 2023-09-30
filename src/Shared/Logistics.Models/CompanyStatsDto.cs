namespace Logistics.Models;

public class CompanyStatsDto
{
    public string? OwnerName { get; set; }
    public int EmployeesCount { get; set; }
    public int ManagersCount { get; set; }
    public int DispatchersCount { get; set; }
    public int DriversCount { get; set; }
    public decimal TotalGross { get; set; }
    public double TotalDistance { get; set; }
}
