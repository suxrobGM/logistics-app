using Logistics.Domain.Specifications;

namespace Logistics.Domain.Repositories;

public static class MainRepositoryExtensions
{
    public static Task<TEntity?> GetAsync<TEntity>(
        this IMainRepository<TEntity> repository,
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot
    {
        return repository.GetAsync(specification.Criteria);
    }

    public static Task<IList<TEntity>> GetListAsync<TEntity>(
        this IMainRepository<TEntity> repository, 
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot
    {
        return repository.GetListAsync(specification.Criteria);
    }

    public static IQueryable<TEntity> GetQuery<TEntity>(
        this IMainRepository<TEntity> repository,
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot
    {
        return repository.GetQuery(specification.Criteria);
    }
}
