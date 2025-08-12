namespace Logistics.Shared.Models;

public record PagedResult<T> : Result<IEnumerable<T>>
{
    public PagedResult() : this(null, 0, 0)
    {
    }

    public PagedResult(IEnumerable<T>? data, int totalItems, int pageSize)
    {
        Data = data;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public static PagedResult<T> Succeed(IEnumerable<T>? items, int totalItems, int totalPages) =>
        new(items, totalItems, totalPages);

    public new static PagedResult<T> Fail(string error) =>
        new(null, 0, 0) { Error = error };
}
