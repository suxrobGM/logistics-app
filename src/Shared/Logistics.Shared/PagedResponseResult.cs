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

    public IEnumerable<T>? Items { get; init; }
    public int ItemsCount { get; init; }
    public int PagesCount { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }

    public bool IsSuccess => string.IsNullOrEmpty(Error);

    public static PagedResponseResult<T> Create(IEnumerable<T>? items, int itemsCount, int pagesCount) =>
        new(items, itemsCount, pagesCount);

    public static PagedResponseResult<T> CreateError(string error) => new(null, 0, 0) { Error = error };
}
