using Logistics.Domain.Entities;
using Logistics.Domain.Exceptions;

namespace Logistics.Application.Services;

/// <summary>
///     Service to manage tenant-related operations.
///     It provides methods to retrieve the current tenant and find tenants by ID.
/// </summary>
public interface ITenantService
{
    /// <summary>
    ///     Get the current tenant set in the context.
    ///     Current tenant ID can be identified by the X-Tenant header from the HTTP request.
    /// </summary>
    /// <returns>
    ///     Current tenant.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the tenant is not set in the context. </exception>
    /// <exception cref="SubscriptionExpiredException">Thrown when the tenant subscription is expired. </exception>
    Tenant GetCurrentTenant();

    /// <summary>
    ///     Find a tenant by ID
    /// </summary>
    /// <param name="tenantId">Tenant ID or Tenant Name</param>
    /// <returns>Tenant entity if found, null otherwise</returns>
    Task<Tenant?> FindTenantByIdAsync(string tenantId);
}
