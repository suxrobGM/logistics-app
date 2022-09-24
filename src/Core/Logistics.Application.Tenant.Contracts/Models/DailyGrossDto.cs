namespace Logistics.Application.Contracts.Models;

public record DailyGrossDto
{
    public DailyGrossDto(DateTime day)
    {
        Day = day;
    }
    
    public DailyGrossDto(int year, int month, int day)
    {
        Day = new DateTime(year, month, day);
    }
    
    public DateTime Day { get; set; }
    public double Gross { get; set; }
    public double Distance { get; set; }
}