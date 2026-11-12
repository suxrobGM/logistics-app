namespace Logistics.Shared.Models;

public class PagedQuery(
    string? orderBy = null,
    int page = 1,
    int pageSize = 10)
{
    public string? OrderBy { get; set; } = orderBy;
    public int Page { get; set; } = page;
    public int PageSize { get; set; } = pageSize;
}
