namespace Logistics.Application.Contracts.Models;

public record MonthlyGrossDto(int Year, int Month)
{
    public double Gross { get; set; }
    public double Distance { get; set; }
}