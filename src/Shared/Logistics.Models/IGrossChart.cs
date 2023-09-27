namespace Logistics.Models;

public interface IGrossChart
{
    public DateTime Date { get; set; }
    public double DriverShare { get; set; }
    public double Gross { get; set; }
    public double Distance { get; set; }
}
