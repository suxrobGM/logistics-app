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
        var accessTenantIds = (await masterUow.Repository<UserTenantAccess>()
                .GetListAsync(a => a.UserId == user.Id, ct))
            .Select(a => a.TenantId)
            .ToList();

        return await masterUow.Repository<Tenant>()
            .GetListAsync(t => t.Id == user.TenantId || accessTenantIds.Contains(t.Id), ct);
    }
}
