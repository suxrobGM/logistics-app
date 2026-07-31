using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

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
        if (string.IsNullOrEmpty(orderBy))
        {
            return query;
        }

        var orderByQuery = CreateOrderQuery<T>(orderBy);
        return orderByQuery is null ? query : DynamicQueryableExtensions.OrderBy(query, orderByQuery);
    }

    /// <summary>
    ///     Null when the field does not exist on <typeparamref name="T" />, leaving the order
    ///     untouched. Sort strings name an <b>entity</b> property, but callers reach for the DTO's
    ///     name often enough (<c>CreatedDate</c> for <c>CreatedAt</c>) that an unknown field must
    ///     degrade quietly - appending a dangling direction builds "descending" on its own, which
    ///     surfaces to the caller as an unrelated parse error deep inside dynamic LINQ.
    /// </summary>
    private static string? CreateOrderQuery<T>(string orderBy)
    {
        var desc = orderBy[0] == '-';
        var prop = desc ? orderBy[1..] : orderBy;

        var matched = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.Name.Equals(prop.Trim(), StringComparison.InvariantCultureIgnoreCase));

        return matched is null ? null : $"{matched.Name} {(desc ? "descending" : "ascending")}";
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
