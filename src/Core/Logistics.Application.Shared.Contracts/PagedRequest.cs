namespace Logistics.Application.Shared;

public abstract class PagedRequest<T> : RequestBase<PagedResponseResult<T>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
