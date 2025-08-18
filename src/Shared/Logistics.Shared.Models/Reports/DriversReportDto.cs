namespace Logistics.Shared.Models;

public record DriverReportDto
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int LoadsDelivered { get; set; }
    public double DistanceDriven { get; set; }
    public decimal GrossEarnings { get; set; }
}

