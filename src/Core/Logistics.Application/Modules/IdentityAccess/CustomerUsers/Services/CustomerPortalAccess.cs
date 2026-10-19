using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.IdentityAccess.CustomerUsers.Services;

/// <summary>
/// Lookups shared by the handlers that keep a tenant's <see cref="CustomerUser" /> rows and their master
/// <see cref="UserTenantAccess" /> rows in step.
/// </summary>
internal static class CustomerPortalAccess
{
    public const string AlreadyActiveMessage =
        "This user already has active portal access for another customer. Deactivate it first.";

    /// <summary>True when the user already has an active portal login for a customer of the current tenant.</summary>
    public static async Task<bool> HasActiveAsync(ITenantUnitOfWork tenantUow, Guid userId, CancellationToken ct) =>
        await tenantUow.Repository<CustomerUser>().GetAsync(cu => cu.UserId == userId && cu.IsActive, ct) is not null;

    /// <summary>The master access row of one portal user of the current tenant.</summary>
    public static Task<UserTenantAccess?> FindAsync(
        ITenantUnitOfWork tenantUow, IMasterUnitOfWork masterUow, Guid customerUserId, CancellationToken ct)
    {
        var tenantId = tenantUow.GetCurrentTenant().Id;
        return masterUow.Repository<UserTenantAccess>()
            .GetAsync(a => a.TenantId == tenantId && a.CustomerUserId == customerUserId, ct);
    }

    /// <summary>The master access rows of every portal user of a customer of the current tenant.</summary>
    public static async Task<List<UserTenantAccess>> ListForCustomerAsync(
        ITenantUnitOfWork tenantUow, IMasterUnitOfWork masterUow, Guid customerId, CancellationToken ct)
    {
        var customerUserIds = await tenantUow.Repository<CustomerUser>().Query()
            .Where(cu => cu.CustomerId == customerId)
            .Select(cu => (Guid?)cu.Id)
            .ToListAsync(ct);

        if (customerUserIds.Count == 0)
        {
            return [];
        }

        var tenantId = tenantUow.GetCurrentTenant().Id;
        return await masterUow.Repository<UserTenantAccess>()
            .GetListAsync(a => a.TenantId == tenantId && customerUserIds.Contains(a.CustomerUserId), ct);
    }
}
