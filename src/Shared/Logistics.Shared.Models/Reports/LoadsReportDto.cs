using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record LoadsReportDto 
{
    public int TotalLoads { get; set; }
    public decimal TotalRevenue { get; set; }
    public double TotalDistance { get; set; }
    public IReadOnlyList<StatusDto> StatusBreakdown { get; set; } = Array.Empty<StatusDto>();
    public IReadOnlyList<TypeDto> TypeBreakdown { get; set; } = Array.Empty<TypeDto>();
    public decimal CancelledLoadsRevenue { get; set; }
    public double CancellationRate { get; set; }
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

