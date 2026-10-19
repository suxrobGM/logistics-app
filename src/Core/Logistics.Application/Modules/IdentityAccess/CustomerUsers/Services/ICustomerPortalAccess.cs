using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.IdentityAccess.CustomerUsers.Services;

/// <summary>
/// Lookups shared by the handlers that keep a tenant's <see cref="CustomerUser" /> rows and their master
/// <see cref="UserTenantAccess" /> rows in step.
/// </summary>
public interface ICustomerPortalAccess : IApplicationService
{
    /// <summary>True when the user already has an active portal login for a customer of the current tenant.</summary>
    Task<bool> HasActiveAsync(Guid userId, CancellationToken ct);

    /// <summary>The master access row of one portal user of the current tenant.</summary>
    Task<UserTenantAccess?> FindAsync(Guid customerUserId, CancellationToken ct);

    /// <summary>The master access rows of every portal user of a customer of the current tenant.</summary>
    Task<List<UserTenantAccess>> ListForCustomerAsync(Guid customerId, CancellationToken ct);
}
