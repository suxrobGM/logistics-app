using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Logistics.Application.Common.Extensions;

public static class QueryableExtensions
{
    /// <summary>Always ends on the unique <paramref name="tieBreaker" />, so paging never repeats or drops rows.</summary>
    public static IOrderedQueryable<T> OrderBy<T, TKey>(
        this IQueryable<T> query,
        string? orderBy,
        Expression<Func<T, TKey>> tieBreaker,
        string? defaultOrderBy = null)
    {
        var effectiveOrderBy = string.IsNullOrEmpty(orderBy) ? defaultOrderBy : orderBy;
        var orderByQuery = string.IsNullOrEmpty(effectiveOrderBy) ? null : CreateOrderQuery<T>(effectiveOrderBy);

        return orderByQuery is null
            ? query.OrderBy(tieBreaker)
            : DynamicQueryableExtensions.OrderBy(query, orderByQuery).ThenBy(tieBreaker);
    }

    /// <summary>Null for an unknown field, because callers often send the DTO's name (CreatedDate for CreatedAt).</summary>
    private static string? CreateOrderQuery<T>(string orderBy)
    {
        var desc = orderBy[0] == '-';
        var path = ResolvePropertyPath(typeof(T), (desc ? orderBy[1..] : orderBy).Trim());

        return path is null ? null : $"{path} {(desc ? "descending" : "ascending")}";
    }

    /// <summary>Resolves segment by segment, since matching the whole dotted path finds no nested property.</summary>
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
