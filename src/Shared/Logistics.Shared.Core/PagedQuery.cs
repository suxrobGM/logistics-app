using System.Text.Json.Serialization;

namespace Logistics.Shared;

public class PagedQuery
{
    private string? _orderBy;
    
    public PagedQuery(int page = 1, int pageSize = 10)
    {
        Page = page;
        PageSize = pageSize;
    }
    
    public string? OrderBy
    {
        get => _orderBy;
        set 
        {
            value ??= string.Empty;

            if (value.StartsWith("-"))
            {
                value = value[1..];
                Descending = true;
            }
            _orderBy = value;
        }
    }
    
    [JsonIgnore]
    public bool Descending { get; private set; }
    
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
