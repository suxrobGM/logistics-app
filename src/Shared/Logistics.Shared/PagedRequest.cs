namespace Logistics.Shared;

public record PagedRequest
{
    public PagedRequest(int page = 1, int pageSize = 10)
    {
        Page = page;
        PageSize = pageSize;
    }
    
    public int Page { get; init; }
    public int PageSize { get; init; }

    public virtual IDictionary<string, string> ToDictionary()
    {
        var queryDict = new Dictionary<string, string>
        {
            {"page", Page.ToString() },
            {"pageSize", PageSize.ToString() }
        };

        return queryDict;
    }
}
