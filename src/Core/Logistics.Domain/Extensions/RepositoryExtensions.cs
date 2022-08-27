using Logistics.Domain.Specifications;

namespace Logistics.Domain.Repositories;

public static class MainRepositoryExtensions
{
    public static IQueryable<TEntity> ApplySpecification<TEntity>(
        this IRepository<TEntity> repository,
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot
    {
        return repository.GetQuery(specification.Criteria)
            .OrderBy(specification.OrderBy);
    }
}
