namespace Logistics.Shared.Models;

public class TrucksSummaryDto
{
    public int TotalCount { get; set; }
    public List<TruckStatusCountDto> StatusCounts { get; set; } = [];
}
