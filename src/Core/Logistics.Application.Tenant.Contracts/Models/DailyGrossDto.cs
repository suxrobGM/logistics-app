namespace Logistics.Application.Tenant.Models;

public record DailyGrossDto
{
    public DailyGrossDto(DateTime date)
    {
        Date = date;
    }
    
    public DailyGrossDto(int year, int month, int day)
    {
        Date = new DateTime(year, month, day);
    }
    
    public DateTime Date { get; set; }
    public double Income { get; set; }
    public double Distance { get; set; }
}