namespace Logistics.Application.Contracts.Queries;
public abstract class SearchableQueryBase<T> : PagedQueryBase<T>
{
    public string? Search { get; set; }
}
