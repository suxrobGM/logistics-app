namespace Logistics.Shared.Models;

public record MonthlyGrossDto : IGrossChartData
{
    public MonthlyGrossDto()
    {
    }

    public MonthlyGrossDto(int year, int month)
    {
        Date = GetLastDayOfMonth(year, month);
    }

    private static DateTime GetLastDayOfMonth(int year, int month)
    {
        // Create a DateTime for the first day of the desired month
        var firstDayOfMonth = new DateTime(year, month, 1);
        return firstDayOfMonth.AddMonths(1).AddDays(-1);
    }

    public DateTime Date { get; set; }
    public decimal DriverShare { get; set; }
    public decimal Gross { get; set; }
    public double Distance { get; set; }
}
