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

    /// <summary>
    ///     Sets the current tenant by ID.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Tenant entity</returns>
    /// <exception cref="InvalidOperationException">Thrown when the tenant is not found.</exception>
    Task<Tenant> SetCurrentTenantByIdAsync(Guid tenantId);

    /// <summary>
    ///     Sets the current tenant directly.
    ///     This is useful when the tenant is already known and does not need to be fetched.
    ///     It should be used with caution to ensure the tenant context is correctly managed.
    /// </summary>
    /// <param name="tenant">Tenant entity</param>
    void SetCurrentTenant(Tenant tenant);
}
