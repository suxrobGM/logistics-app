namespace Logistics.Shared.Models;

public class CompanyStatsDto
{
    public string? OwnerName { get; set; }
    public int EmployeesCount { get; set; }
    public int ManagersCount { get; set; }
    public int DispatchersCount { get; set; }
    public int DriversCount { get; set; }
    public int TrucksCount { get; set; }
    
    public decimal ThisWeekGross { get; set; }
    public double ThisWeekDistance { get; set; }
    
    public decimal LastWeekGross { get; set; }
    public double LastWeekDistance { get; set; }
    
    public decimal ThisMonthGross { get; set; }
    public double ThisMonthDistance { get; set; }
    
    public decimal LastMonthGross { get; set; }
    public double LastMonthDistance { get; set; }
    
    public decimal TotalGross { get; set; }
    public double TotalDistance { get; set; }
}
