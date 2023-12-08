namespace Logistics.Domain.Extensions;

public static class EnumerableExtensions
{
    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
        this IEnumerable<TSource> query,
        Func<TSource, TKey> keySelector,
        bool isDescendingOrder)
    {
        return isDescendingOrder ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
        
    public static IEnumerable<TSource> ApplyPaging<TSource>(
        this IEnumerable<TSource> query,
        int page,
        int pageSize)
    {
        return query.Skip((page - 1) * pageSize)
            .Take(pageSize);
    }
}
