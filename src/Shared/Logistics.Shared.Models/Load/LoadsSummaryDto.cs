namespace Logistics.Shared.Models;

public class LoadsSummaryDto
{
    public int TotalCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<LoadStatusCountDto> StatusCounts { get; set; } = [];
}
