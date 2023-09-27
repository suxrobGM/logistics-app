namespace Logistics.Models;

public interface IGrossChart
{
    public DateTime Date { get; set; }
    public decimal DriverShare { get; set; }
    public decimal Gross { get; set; }
    public double Distance { get; set; }
}
