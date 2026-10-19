using Logistics.Application.Abstractions.Tenancy;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Infrastructure.Persistence.Reads;

internal sealed class UserTenantReader(IMasterUnitOfWork masterUow) : IUserTenantReader
{
    public async Task<List<Tenant>> ListAsync(User user, CancellationToken ct)
    {
        var accesses = masterUow.Repository<UserTenantAccess>().Query();
        return await masterUow.Repository<Tenant>().GetListAsync(
            t => t.Id == user.TenantId || accesses.Any(a => a.UserId == user.Id && a.TenantId == t.Id), ct);
    }
}
