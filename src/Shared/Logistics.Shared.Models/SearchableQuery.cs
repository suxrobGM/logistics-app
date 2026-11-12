namespace Logistics.Shared.Models;

public class SearchableQuery(
    string? search = null,
    string? orderBy = null,
    int page = 1,
    int pageSize = 10) : PagedQuery(orderBy, page, pageSize)
{
    public string? Search { get; init; } = search;
}
