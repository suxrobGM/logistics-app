namespace Logistics.Shared.Models;

/// <summary>Rows are sorted worst margin first.</summary>
public record ProfitabilityReportDto
{
    public ProfitabilitySummaryDto Summary { get; set; } = new();
    public IReadOnlyList<TruckProfitDto> Trucks { get; set; } = [];
    public IReadOnlyList<LaneProfitDto> Lanes { get; set; } = [];
    public IReadOnlyList<LoadProfitDto> Loads { get; set; } = [];
}
