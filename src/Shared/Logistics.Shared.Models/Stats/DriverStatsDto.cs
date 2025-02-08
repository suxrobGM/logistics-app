namespace Logistics.Shared.Models;

public class DriverStatsDto
{
    public decimal ThisWeekGross { get; set; }
    public decimal ThisWeekShare { get; set; }
    public double ThisWeekDistance { get; set; }
    
    public decimal LastWeekGross { get; set; }
    public decimal LastWeekShare { get; set; }
    public double LastWeekDistance { get; set; }
    
    public decimal ThisMonthGross { get; set; }
    public decimal ThisMonthShare { get; set; }
    public double ThisMonthDistance { get; set; }
    
    public decimal LastMonthGross { get; set; }
    public decimal LastMonthShare { get; set; }
    public double LastMonthDistance { get; set; }
}
