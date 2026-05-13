using Logistics.Domain.Entities;
using Logistics.Domain.Exceptions;
using Logistics.Application.Abstractions.Tenancy;

namespace Logistics.Application.Abstractions.Tenancy;

/// <summary>
///     Resolves the tenant for the current scope (HTTP request, background job,
///     etc.) from the ambient context — X-Tenant header, MCP API key, JWT claim,
///     or a default fallback. Roughly analogous to <c>IHttpContextAccessor</c>
///     but scoped to the multi-tenant model.
/// </summary>
/// <remarks>
///     For switching the tenant context within a scope (e.g. background jobs
///     iterating tenants), use <see cref="ITenantUnitOfWork"/> — it is the single
///     point that owns and mutates the current-tenant state.
/// </remarks>
public interface ICurrentTenantAccessor
{
    /// <summary>
    ///     Get the current tenant set in the context.
    /// </summary>
    /// <returns>Current tenant.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the tenant is not set in the context.</exception>
    /// <exception cref="SubscriptionExpiredException">Thrown when the tenant subscription is expired.</exception>
    Tenant GetCurrentTenant();
}
