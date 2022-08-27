using System.Linq.Expressions;

namespace Logistics.Domain.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> Sort<TSource>(
        this IEnumerable<TSource> query,
        Func<TSource, object> keySelector, 
        bool descending)
    {
        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}