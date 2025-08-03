using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Logistics.Infrastructure.Persistence;

internal static class PathHelper
{
    private static readonly ConcurrentDictionary<(Type,string), LambdaExpression> Cache = new();

    /// <summary>
    /// Return an x => x.Foo.Bar selector for the given dotted path.
    /// </summary>
    public static Expression<Func<T, object?>> BuildSelector<T>(string path)
    {
        return (Expression<Func<T, object?>>)Cache.GetOrAdd((typeof(T), path), key =>
        {
            var (rootType, rawPath) = key;

            var parameter = Expression.Parameter(rootType, "x");
            Expression body = parameter;
            var current = rootType;

            foreach (var segment in rawPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                var prop = current.GetProperty(segment,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                if (prop is null)
                    throw new ArgumentException(
                        $"Property '{segment}' not found on '{current.Name}'", nameof(path));

                body = Expression.Property(body, prop);
                current = prop.PropertyType;
            }

            body = Expression.Convert(body, typeof(object));
            return Expression.Lambda(body, parameter);
        });
    }

    /// <summary>
    /// True when the path is a single segment and exactly matches a CLR property.
    /// </summary>
    public static bool IsSingleExactSegment<T>(string path)
    {
        if (path.Contains('.'))
        {
            return false;
        }
        
        return typeof(T).GetProperty(path,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) is not null;
    }
}
