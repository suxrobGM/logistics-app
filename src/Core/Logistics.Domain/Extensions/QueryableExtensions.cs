using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Logistics.Domain.Persistence;

public static class QueryableExtensions
{
    /// <summary>
    ///     Shortcut for ordering a queryable source by a key selector.
    ///     It allows specifying whether the order should be ascending or descending.
    /// </summary>
    /// <param name="query">The queryable source to order.</param>
    /// <param name="keySelector">The key selector expression to determine the order.</param>
    /// <param name="descending">A boolean indicating whether to order in descending order (true) or ascending order (false).</param>
    public static IQueryable<TSource> OrderBy<TSource, TKey>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TKey>> keySelector,
        bool descending)
    {
        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }

    /// <summary>
    ///     Shortcut for ordering a queryable source by a string.
    /// </summary>
    /// <param name="query">The queryable source to order.</param>
    /// <param name="orderBy">the string to determine the order.</param>
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string? orderBy)
    {
        if (!string.IsNullOrEmpty(orderBy))
        {
            var orderByQuery = CreateOrderQuery<T>(orderBy);
            return DynamicQueryableExtensions.OrderBy(query, orderByQuery);
        }

        return query;
    }

    private static string CreateOrderQuery<T>(string orderBy)
    {
        var desc = orderBy[0] == '-';
        var prop = desc ? orderBy[1..] : orderBy;

        var orderByQueryBuilder = new StringBuilder();
        var orderProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var matched = orderProps
            .FirstOrDefault(p => p.Name.Equals(prop.Trim(), StringComparison.InvariantCultureIgnoreCase));

        var dir = desc ? "descending" : "ascending";
        orderByQueryBuilder.Append($"{matched?.Name} {dir}");

        var orderQuery = orderByQueryBuilder.ToString().Trim();
        return orderQuery;
    }

    /// <summary>
    ///     Applies paging to the queryable source based on the specified page and page size.
    ///     The page is 1-based, meaning that page 1 corresponds to the first set of results.
    /// </summary>
    /// <param name="query">The queryable source to apply paging to.</param>
    /// <param name="page">The page number to retrieve, starting from 1.</param>
    /// <param name="pageSize">The number of items to include in each page.</param>
    public static IQueryable<TSource> ApplyPaging<TSource>(
        this IQueryable<TSource> query,
        int page,
        int pageSize)
    {
        return query.Skip((page - 1) * pageSize)
            .Take(pageSize);
    }
}
