namespace Logistics.Application.Shared;

public abstract class SearchableQuery<T> : PagedQuery<T>
{
    private string? _orderBy;

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
    
    public bool Descending { get; private set; }
    public string? Search { get; set; }
}
