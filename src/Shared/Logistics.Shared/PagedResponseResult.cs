using System.Text.Json.Serialization;

namespace Logistics.Shared;

public record PagedResponseResult<T> : IResponseResult
{
    public PagedResponseResult(): this(null, 0, 0)
    {
        
    }

    public PagedResponseResult(IEnumerable<T>? items, int itemsCount, int pagesCount)
    {
        Items = items;
        ItemsCount = itemsCount;
        PagesCount = pagesCount;
    }

    public IEnumerable<T>? Items { get; }
    public int ItemsCount { get; }
    public int PagesCount { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }

    public bool Success => string.IsNullOrEmpty(Error);

    public static PagedResponseResult<T> CreateError(string error) => new(null, 0, 0) { Error = error };
}
