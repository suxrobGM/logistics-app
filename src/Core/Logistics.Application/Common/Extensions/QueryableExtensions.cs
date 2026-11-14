namespace Logistics.Application.Common.Extensions;

public static class QueryableExtensions
{
    /// <summary>The page is 1-based.</summary>
    public static IQueryable<TSource> ApplyPaging<TSource>(
        this IQueryable<TSource> query,
        int page,
        int pageSize)
    {
        return query.Skip((page - 1) * pageSize)
            .Take(pageSize);
    }
}
