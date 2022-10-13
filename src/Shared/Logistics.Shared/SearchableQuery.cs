namespace Logistics.Shared;

public record SearchableQuery : PagedQuery
{
    public SearchableQuery(string? search = null)
    {
        Search = search;
    }

    private string? _orderBy;

    public string? OrderBy
    {
        get => _orderBy;
        set
        {
            value ??= string.Empty;

            if (value.StartsWith("-"))
                value = value[1..];
            
            _orderBy = value;
        }
    }

    public string? Search { get; init; }
}
