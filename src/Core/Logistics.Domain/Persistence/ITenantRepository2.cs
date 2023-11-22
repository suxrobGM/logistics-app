using Logistics.Domain.Core;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Persistence;

/// <summary>
/// Tenant's repository.
/// </summary>
public interface ITenantRepository2<TEntity> : IMasterRepository2<TEntity> where TEntity : class, ITenantEntity
{
    /// <summary>
    /// Gets the current tenant data.
    /// </summary>
    Tenant GetCurrentTenant();

    /// <summary>
    /// Manually set the current tenant by its ID
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    void SetCurrentTenantById(string tenantId);
    
    /// <summary>
    /// Manually set the current tenant by directly passing the instance
    /// </summary>
    /// <param name="tenant">An instance of Tenant class</param>
    void SetCurrentTenant(Tenant tenant);
}
