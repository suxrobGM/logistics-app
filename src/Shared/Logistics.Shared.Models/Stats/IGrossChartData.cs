namespace Logistics.Shared.Models;

public interface IGrossChartData
{
    public DateTime Date { get; set; }
    public decimal DriverShare { get; set; }
    public decimal Gross { get; set; }
    public double Distance { get; set; }
}
