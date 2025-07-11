using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

internal static class OrderByBuilder
{
    private static readonly ConcurrentDictionary<(Type, string), LambdaExpression> Cache = new();

    public static Expression<Func<T, object?>> Build<T>(string propertyPath)
    {
        return (Expression<Func<T, object?>>)Cache.GetOrAdd((typeof(T), propertyPath), _ =>
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression body = parameter;

            foreach (var member in propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                body = Expression.PropertyOrField(body, member);
            }
            
            // Cast so every projection fits Expression<Func<T, object?>>
            body = Expression.Convert(body, typeof(object));
            return Expression.Lambda(body, parameter);
        });
    }
}
