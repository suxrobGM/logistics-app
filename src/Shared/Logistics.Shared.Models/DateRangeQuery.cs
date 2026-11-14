namespace Logistics.Shared.Models;

public class DateRangeQuery
{
    public DateTime FromDate { get; set; } = DateTime.UtcNow;
    public DateTime ToDate { get; set; } = DateTime.UtcNow;
}
