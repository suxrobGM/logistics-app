namespace Logistics.Shared.Models;

public class PagedQuery(
    string? orderBy = null,
    int page = 1,
    int pageSize = 10)
{
    public string? OrderBy { get; set; } = orderBy;
    public int Page { get; set; } = page;
    public int PageSize { get; set; } = pageSize;

    public virtual IDictionary<string, string> ToDictionary()
    {
        var queryDict = new Dictionary<string, string>
        {
            {"page", Page.ToString()},
            {"pageSize", PageSize.ToString()}
        };

        if (!string.IsNullOrEmpty(OrderBy))
        {
            queryDict.Add("orderBy", OrderBy);
        }

        return queryDict;
    }
}
