namespace Logistics.Shared.Models;

public class PagedQuery
{
    public PagedQuery(
        string? orderBy = null,
        int page = 1,
        int pageSize = 10)
    {
        OrderBy = orderBy;
        Page = page;
        PageSize = pageSize;
    }
    
    public string? OrderBy { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    
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
