namespace Logistics.Domain.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TSource> OrderBy<TSource>(
        this IQueryable<TSource> query, string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return query;
        
        var entityType = typeof(TSource);

        //Create x=>x.PropName
        var propertyInfo = entityType.GetProperty(propertyName);

        if (propertyInfo == null)
            return query;

        var arg = Expression.Parameter(entityType, "x");
        var property = Expression.Property(arg, propertyName);
        var selector = Expression.Lambda(property, arg);

        //Get System.Linq.Queryable.OrderBy() method.
        var enumerableType = typeof(Queryable);
        var method = enumerableType.GetMethods()
            .Where(m => m.Name == "OrderBy" && m.IsGenericMethodDefinition)
            .Where(m =>
            {
                var parameters = m.GetParameters().ToList();
                //Put more restriction here to ensure selecting the right overload                
                return parameters.Count == 2;//overload that has 2 parameters
            }).Single();
        
        //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
        var genericMethod = method.MakeGenericMethod(entityType, propertyInfo.PropertyType);
        
        /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
          Note that we pass the selector as Expression to the method and we don't compile it.
          By doing so EF can extract "order by" columns and generate SQL for it.*/
        var newQuery = genericMethod.Invoke(genericMethod, new object[] { query, selector });

        if (newQuery is IOrderedQueryable<TSource> orderedQuery)
        {
            return orderedQuery;
        }
        
        return query;
    }
}