namespace Logistics.Shared.Models;

public class TripsSummaryDto
{
    public int TotalCount { get; set; }
    public List<TripStatusCountDto> StatusCounts { get; set; } = [];
}
