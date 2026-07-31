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
    ///     Null leaves the order untouched. Sort strings name an entity property, but callers reach
    ///     for the DTO's (<c>CreatedDate</c> for <c>CreatedAt</c>) often enough that an unknown one
    ///     must not become a dynamic-LINQ parse error.
    /// </summary>
    private static string? CreateOrderQuery<T>(string orderBy)
    {
        var desc = orderBy[0] == '-';
        var path = ResolvePropertyPath(typeof(T), (desc ? orderBy[1..] : orderBy).Trim());

        return path is null ? null : $"{path} {(desc ? "descending" : "ascending")}";
    }

    /// <summary>
    ///     Resolves a dotted path (<c>Customer.Name</c>) segment by segment, in each property's own
    ///     casing. Matching the whole path as a single name resolves no nested sort at all.
    /// </summary>
    private static string? ResolvePropertyPath(Type type, string path)
    {
        var canonical = new List<string>();
        var current = type;

        foreach (var segment in path.Split('.'))
        {
            var property = current
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.Name.Equals(segment, StringComparison.InvariantCultureIgnoreCase));

            if (property is null)
            {
                return null;
            }

            canonical.Add(property.Name);
            current = property.PropertyType;
        }

        return string.Join('.', canonical);
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
