namespace Logistics.Application.Shared;

public abstract class SearchableQueryBase<T> : PagedQueryBase<T>
{
    public string? Search { get; set; }
}
