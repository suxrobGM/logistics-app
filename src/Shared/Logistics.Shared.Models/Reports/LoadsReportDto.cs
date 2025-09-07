using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record LoadsReportDto
{
    public int TotalLoads { get; set; }
    public decimal TotalRevenue { get; set; }
    public double TotalDistance { get; set; }
    public decimal AverageRevenuePerLoad { get; set; }
    public double AverageDistancePerLoad { get; set; }
    public IReadOnlyList<StatusDto> StatusBreakdown { get; set; } = Array.Empty<StatusDto>();
    public IReadOnlyList<TypeDto> TypeBreakdown { get; set; } = Array.Empty<TypeDto>();
    public decimal CancelledLoadsRevenue { get; set; }
    public double CancellationRate { get; set; }
    public IReadOnlyList<LoadTrendDto> LoadTrends { get; set; } = Array.Empty<LoadTrendDto>();
    public IReadOnlyList<TopCustomerLoadDto> TopCustomers { get; set; } = Array.Empty<TopCustomerLoadDto>();
    public IReadOnlyList<LoadPerformanceDto> PerformanceMetrics { get; set; } = Array.Empty<LoadPerformanceDto>();
}


public class StatusDto
{
    public LoadStatus Status { get; set; }
    public int Count { get; set; }
    public decimal TotalRevenue { get; set; }
}
public class TypeDto
{
    public LoadType Type { get; set; }
    public int Count { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class LoadTrendDto
{
    public string Period { get; set; } = string.Empty;
    public int LoadCount { get; set; }
    public decimal Revenue { get; set; }
    public double Distance { get; set; }
}

public class TopCustomerLoadDto
{
    public string CustomerName { get; set; } = string.Empty;
    public int LoadCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public double TotalDistance { get; set; }
    public double AverageDistance { get; set; }
}

public class LoadPerformanceDto
{
    public string Metric { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double Trend { get; set; } // Percentage change from previous period
}
