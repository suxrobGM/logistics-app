using Logistics.Domain.Specifications;

namespace Logistics.Domain.Repositories;

public static class TenantRepositoryExtensions
{
    public static Task<TEntity?> GetAsync<TEntity>(
        this ITenantRepository<TEntity> repository,
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot, ITenantEntity
    {
        return repository.GetAsync(specification.Criteria);
    }

    public static Task<IList<TEntity>> GetListAsync<TEntity>(
        this ITenantRepository<TEntity> repository, 
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot, ITenantEntity
    {
        return repository.GetListAsync(specification.Criteria);
    }

    public static IQueryable<TEntity> GetQuery<TEntity>(
        this ITenantRepository<TEntity> repository,
        ISpecification<TEntity> specification)
        where TEntity : class, IAggregateRoot, ITenantEntity
    {
        return repository.GetQuery(specification.Criteria);
    }
}
