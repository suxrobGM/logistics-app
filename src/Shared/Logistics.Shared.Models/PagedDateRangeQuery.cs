namespace Logistics.Shared.Models;

public class PagedDateRangeQuery : PagedQuery
{
    public DateTime FromDate { get; set; } = DateTime.UtcNow;
    public DateTime ToDate { get; set; } = DateTime.UtcNow;
}
