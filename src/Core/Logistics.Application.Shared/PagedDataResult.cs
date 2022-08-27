using System.Text.Json.Serialization;

namespace Logistics.Application.Shared;

public class PagedDataResult<T> : IDataResult
{
    public PagedDataResult()
    {
    }

    public PagedDataResult(IEnumerable<T> items, int itemsCount, int pagesCount)
    {
        Items = items;
        ItemsCount = itemsCount;
        PagesCount = pagesCount;
    }

    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int ItemsCount { get; set; }
    public int PagesCount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }
    
    public bool Success => string.IsNullOrEmpty(Error);
    
    public static PagedDataResult<T> CreateError(string error) => new() { Error = error };
}
