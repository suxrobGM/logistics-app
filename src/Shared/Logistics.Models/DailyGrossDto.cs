namespace Logistics.Models;

public record DailyGrossDto : IGrossChart
{
    public DailyGrossDto()
    {
    }
    
    public DailyGrossDto(int year, int month, int day)
    {
        Date = new DateTime(year, month, day);
    }
    
    public DateTime Date { get; set; }
    public double DriverShare { get; set; }
    public double Gross { get; set; }
    public double Distance { get; set; }
}
