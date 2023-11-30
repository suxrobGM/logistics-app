namespace Logistics.Shared;

public class PagedResponseResult<T> : ResponseResult<IEnumerable<T>>
{
    public PagedResponseResult(): this(null, 0, 0)
    {
    }

    public PagedResponseResult(IEnumerable<T>? data, int totalItems, int totalPages)
    {
        Data = data;
        TotalItems = totalItems;
        TotalPages = totalPages;
    }
    
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public static PagedResponseResult<T> Create(IEnumerable<T>? items, int totalItems, int totalPages) =>
        new(items, totalItems, totalPages);

    public new static PagedResponseResult<T> CreateError(string error) => new(null, 0, 0) { Error = error };
}
