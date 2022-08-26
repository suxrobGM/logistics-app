namespace Logistics.Application.Shared;

public class PagedDataResult<T> : DataResult<T>
{
    public PagedDataResult()
    {
    }

    public PagedDataResult(T[] items, int itemsCount, int pagesCount)
    {
        Items = items;
        ItemsCount = itemsCount;
        PagesCount = pagesCount;
    }

    public T[]? Items { get; set; }
    public int ItemsCount { get; set; }
    public int PagesCount { get; set; }
    
    public new static PagedDataResult<T> CreateError(string error) => new() { Error = error };
}
