namespace Logistics.Application.Contracts;

public class PagedDataResult<T> : DataResult<T>
{
    public PagedDataResult()
    {
    }

    public PagedDataResult(T[] items, int totalItems, int totalPages)
    {
        Items = items;
        TotalItems = totalItems;
        TotalPages = totalPages;
    }

    public T[]? Items { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
