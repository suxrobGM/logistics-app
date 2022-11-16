using Logistics.Domain.Specifications;

namespace Logistics.Domain.Persistence;

public static class RepositoryExtensions
{
    public static IQueryable<TEntity> ApplySpecification<TEntity>(
        this IRepository repository,
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot
    {
        var query = repository.Query(specification.Criteria);
        
        return specification.Descending ? 
            query.OrderByDescending(specification.OrderBy)
            : query.OrderBy(specification.OrderBy);
    }
}
