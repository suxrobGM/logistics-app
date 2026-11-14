namespace Logistics.Shared.Models;

public class PagedDateRangeQuery : PagedQuery
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
