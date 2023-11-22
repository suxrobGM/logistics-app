using Logistics.Domain.Core;
using Logistics.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.EF.Persistence;

public static class SpecificationEvaluator<TEntity> where TEntity : class, IEntity<string>
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
    {
        var query = inputQuery;

        // modify the IQueryable using the specification's criteria expression
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }
        
        // Includes all expression-based includes
        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        // Include any string-based include statements
        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        // Apply ordering if expressions are set
        if (specification.OrderBy != null)
        {
            query = specification.Descending
                ? query.OrderByDescending(specification.OrderBy)
                : query.OrderBy(specification.OrderBy);
        }

        if (specification.GroupBy != null)
        {
            query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
        }

        // Apply paging if enabled
        if (specification.IsPagingEnabled)
        {
            query = query.Skip((specification.Page - 1) * specification.PageSize)
                .Take(specification.PageSize);
        }
        return query;
    }
}
