using Logistics.Domain.Core;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Persistence;

/// <summary>
///     Unit of Work for tenant-scoped entities.
/// </summary>
public interface ITenantUnitOfWork : IUnitOfWork<ITenantEntity>
{
    /// <inheritdoc cref="IUnitOfWork{TMarker}.Repository{TEntity}" />
    new ITenantRepository<TEntity, Guid> Repository<TEntity>()
        where TEntity : class, IEntity<Guid>, ITenantEntity;

    /// <inheritdoc cref="IUnitOfWork{TMarker}.Repository{TEntity, TKey}" />
    new ITenantRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>, ITenantEntity;

    /// <summary>Gets the currently selected tenant.</summary>
    Tenant GetCurrentTenant();

    /// <summary>Sets the current tenant by ID.</summary>
    void SetCurrentTenantById(string tenantId);

    /// <summary>Sets the current tenant by instance.</summary>
    void SetCurrentTenant(Tenant tenant);
}
