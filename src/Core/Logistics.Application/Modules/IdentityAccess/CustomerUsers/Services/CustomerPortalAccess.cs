using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.IdentityAccess.CustomerUsers.Services;

internal sealed class CustomerPortalAccess(ITenantUnitOfWork tenantUow, IMasterUnitOfWork masterUow)
    : ICustomerPortalAccess
{
    public const string AlreadyActiveMessage =
        "This user already has active portal access for another customer. Deactivate it first.";

    public async Task<bool> HasActiveAsync(Guid userId, CancellationToken ct) =>
        await tenantUow.Repository<CustomerUser>().GetAsync(cu => cu.UserId == userId && cu.IsActive, ct) is not null;

    public Task<UserTenantAccess?> FindAsync(Guid customerUserId, CancellationToken ct)
    {
        var tenantId = tenantUow.GetCurrentTenant().Id;
        return masterUow.Repository<UserTenantAccess>()
            .GetAsync(a => a.TenantId == tenantId && a.CustomerUserId == customerUserId, ct);
    }

    public async Task<List<UserTenantAccess>> ListForCustomerAsync(Guid customerId, CancellationToken ct)
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
