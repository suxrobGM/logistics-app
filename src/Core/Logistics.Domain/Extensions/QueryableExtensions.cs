using Logistics.Domain.Core;
using Logistics.Domain.Specifications;

namespace Logistics.Domain.Persistence;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> ApplySpecification<TEntity>(
        this IQueryable<TEntity> queryable,
        ISpecification<TEntity> specification)
        where TEntity : class, IEntity<string>
    {
        var query = queryable;
        if (specification.Criteria is not null)
        {
            query = queryable.Where(specification.Criteria);
        }

        if (specification.OrderBy is not null)
        {
            query = specification.Descending ? 
                query.OrderByDescending(specification.OrderBy)
                : query.OrderBy(specification.OrderBy);
        }
        
        if (specification.IsPagingEnabled)
        {
            query = query.Skip((specification.Page - 1) * specification.PageSize)
                .Take(specification.PageSize);
        }

        return query;
    }
}
