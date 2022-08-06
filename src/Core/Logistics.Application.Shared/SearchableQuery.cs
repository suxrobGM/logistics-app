namespace Logistics.Application.Shared;

public abstract class SearchableQuery<T> : PagedQuery<T>
{
    public string? Search { get; set; }
}
