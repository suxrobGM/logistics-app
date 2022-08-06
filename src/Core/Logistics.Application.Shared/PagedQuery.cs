namespace Logistics.Application.Shared;

public abstract class PagedQuery<T> : RequestBase<PagedDataResult<T>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
