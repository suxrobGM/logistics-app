namespace Logistics.Application.Contracts.Models;

public record DailyGrossDto(DateTime Date)
{
    public decimal Gross { get; set; }
    public double Distance { get; set; }
}