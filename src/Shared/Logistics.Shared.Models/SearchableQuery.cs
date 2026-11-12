namespace Logistics.Shared.Models;

public class SearchableQuery(
    string? search = null,
    string? orderBy = null,
    int page = 1,
    int pageSize = 10) : PagedQuery(orderBy, page, pageSize)
{
    public string? Search { get; init; } = search;

    public override IDictionary<string, string> ToDictionary()
    {
        var queryDict = base.ToDictionary();

        if (!string.IsNullOrEmpty(Search))
        {
            queryDict.Add("search", Search);
        }

        return queryDict;
    }
}
