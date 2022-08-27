using System.Text.Json.Serialization;

namespace Logistics.Application.Shared;

public class PagedDataResult<T> : IDataResult
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

    public T[] Items { get; set; } = Array.Empty<T>();
    public int ItemsCount { get; set; }
    public int PagesCount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }
    
    public bool Success => string.IsNullOrEmpty(Error);
    
    public static PagedDataResult<T> CreateError(string error) => new() { Error = error };
}
