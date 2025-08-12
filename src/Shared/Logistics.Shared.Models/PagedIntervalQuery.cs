namespace Logistics.Shared.Models;

public class PagedIntervalQuery : PagedQuery
{
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
}
