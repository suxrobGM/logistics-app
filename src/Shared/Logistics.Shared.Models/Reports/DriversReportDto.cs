namespace Logistics.Shared.Models;

public record DriverReportDto
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int LoadsDelivered { get; set; }
    public double DistanceDriven { get; set; }
    public decimal GrossEarnings { get; set; }
    public double AverageDistancePerLoad { get; set; }
    public decimal AverageEarningsPerLoad { get; set; }
    public double Efficiency { get; set; } // Loads per day
    public string TruckNumber { get; set; } = string.Empty;
    public bool IsMainDriver { get; set; }
}

public record DriverDashboardDto
{
    public int TotalDrivers { get; set; }
    public int ActiveDrivers { get; set; }
    public decimal TotalEarnings { get; set; }
    public double TotalDistance { get; set; }
    public int TotalLoadsDelivered { get; set; }
    public decimal AverageEarningsPerDriver { get; set; }
    public double AverageDistancePerDriver { get; set; }
    public IReadOnlyList<DriverPerformanceDto> TopPerformers { get; set; } = Array.Empty<DriverPerformanceDto>();
    public IReadOnlyList<DriverTrendDto> DriverTrends { get; set; } = Array.Empty<DriverTrendDto>();
    public IReadOnlyList<DriverEfficiencyDto> EfficiencyMetrics { get; set; } = Array.Empty<DriverEfficiencyDto>();
}

public class DriverPerformanceDto
{
    public string DriverName { get; set; } = string.Empty;
    public int LoadsDelivered { get; set; }
    public decimal Earnings { get; set; }
    public double Distance { get; set; }
    public double Efficiency { get; set; }
}

public class DriverTrendDto
{
    public string Period { get; set; } = string.Empty;
    public int ActiveDrivers { get; set; }
    public int LoadsDelivered { get; set; }
    public decimal TotalEarnings { get; set; }
    public double TotalDistance { get; set; }
}

public class DriverEfficiencyDto
{
    public string Metric { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double Trend { get; set; }
}
