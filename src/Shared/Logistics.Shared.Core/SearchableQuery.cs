namespace Logistics.Shared;

public class SearchableQuery : PagedQuery
{
    public SearchableQuery(string? search = null, int page = 1, int pageSize = 10) 
        : base(page, pageSize)
    {
        Search = search;
    }
    
    public string? Search { get; init; }

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
