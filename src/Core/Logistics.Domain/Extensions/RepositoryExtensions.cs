using Logistics.Domain.Specifications;

namespace Logistics.Domain.Repositories;

public static class RepositoryExtensions
{
    public static Task<TEntity?> GetAsync<TEntity>(
        this IRepository<TEntity> repository,
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot
    {
        return repository.GetAsync(specification.Criteria);
    }

    public static Task<IList<TEntity>> GetListAsync<TEntity>(
        this IRepository<TEntity> repository, 
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot
    {
        return repository.GetListAsync(specification.Criteria);
    }

    public static IQueryable<TEntity> GetQuery<TEntity>(
        this IRepository<TEntity> repository,
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot
    {
        return repository.GetQuery(specification.Criteria);
    }
}
