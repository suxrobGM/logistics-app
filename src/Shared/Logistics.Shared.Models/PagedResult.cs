namespace Logistics.Shared.Models;

public record PagedResult<T> : Result<IEnumerable<T>>
{
    public PagedResult() : this(null, 0, 0)
    {
    }

    public PagedResult(IEnumerable<T>? value, int totalItems, int pageSize): base(value)
    {
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public static PagedResult<T> Ok(IEnumerable<T>? items, int totalItems, int pageSize)
    {
        return new PagedResult<T>(items, totalItems, pageSize);
    }

    public new static PagedResult<T> Fail(string error)
    {
        return new PagedResult<T>(null, 0, 0) { Error = error };
    }
}
