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
        var query = queryable.Where(specification.Criteria);
        
        return specification.Descending ? 
            query.OrderByDescending(specification.OrderBy)
            : query.OrderBy(specification.OrderBy);
    }
    
    public static IQueryable<TEntity> ApplySpecification<TEntity>(
        this IRepository repository,
        ISpecification<TEntity> specification)
        where TEntity : class, IEntity<string>
    {
        var query = repository.Query<TEntity>().Where(specification.Criteria);
        
        return specification.Descending ? 
            query.OrderByDescending(specification.OrderBy)
            : query.OrderBy(specification.OrderBy);
    }
}
