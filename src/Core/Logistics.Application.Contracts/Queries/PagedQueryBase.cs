namespace Logistics.Application.Contracts.Queries;

public abstract class PagedQueryBase<T> : RequestBase<PagedDataResult<T>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
