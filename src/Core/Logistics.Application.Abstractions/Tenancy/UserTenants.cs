using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Abstractions.Tenancy;

public static class UserTenants
{
    /// <summary>
    ///     Every tenant holding rows for the user: their own tenant (<see cref="User.TenantId" />) plus each
    ///     tenant they reach through <see cref="UserTenantAccess" /> (customer portal access).
    /// </summary>
    public static async Task<List<Tenant>> ListAsync(IMasterUnitOfWork masterUow, User user, CancellationToken ct)
    {
        var accesses = masterUow.Repository<UserTenantAccess>().Query();
        return await masterUow.Repository<Tenant>().GetListAsync(
            t => t.Id == user.TenantId || accesses.Any(a => a.UserId == user.Id && a.TenantId == t.Id), ct);
    }
}
