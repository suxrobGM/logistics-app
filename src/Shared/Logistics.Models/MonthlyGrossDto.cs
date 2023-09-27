namespace Logistics.Models;

public record MonthlyGrossDto
{
    public MonthlyGrossDto()
    {
    }

    public MonthlyGrossDto(int year, int month)
    {
        Year = year;
        Month = month;
    }
    
    public int Year { get; set; }
    public int Month { get; set; }
    public double Income { get; set; }
    public double Distance { get; set; }
}
