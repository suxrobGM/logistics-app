namespace Logistics.Application.Contracts;

public class PagedDataResult<T> : DataResult<T>
{
    public PagedDataResult() : this(Array.Empty<T>(), 0, 0)
    {
    }

    public PagedDataResult(T[] items, int totalItems, int totalPages)
    {
        Items = items;
        TotalItems = totalItems;
        TotalPages = totalPages;
    }

    public T[] Items { get; }
    public int TotalItems { get; }
    public int TotalPages { get; }
}
