namespace Logistics.Application.Shared;

public abstract class SearchableQuery<T> : PagedQuery<T>
{
    private string? _orderBy;

    public string? OrderBy
    {
        get => _orderBy;
        set => _orderBy = value.Capitalize();
    }
    public string? Search { get; set; }
}
