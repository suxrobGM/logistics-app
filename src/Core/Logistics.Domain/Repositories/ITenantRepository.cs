namespace Logistics.Domain.Repositories;

/// <summary>
/// Tenant's repository.
/// </summary>
/// <typeparam name="TEntity">Class that implements <see cref="IAggregateRoot"/> and 
/// <see cref="ITenantEntity"/> interface
/// </typeparam>
public interface ITenantRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IAggregateRoot, ITenantEntity
{
    /// <summary>
    /// Tenant's UOW
    /// </summary>
    ITenantUnitOfWork UnitOfWork { get; }
    
    Tenant? CurrentTenant { get; }
}