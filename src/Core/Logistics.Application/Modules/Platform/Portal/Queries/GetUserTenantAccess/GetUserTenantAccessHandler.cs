using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

internal sealed class GetUserTenantAccessHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetUserTenantAccessQuery, Result<List<UserTenantAccessDto>>>
{
    public async Task<Result<List<UserTenantAccessDto>>> Handle(
        GetUserTenantAccessQuery req,
        CancellationToken ct)
    {
        var tenantAccessList = await masterUow.Repository<UserTenantAccess>().Query()
            .Where(uta => uta.UserId == req.UserId && uta.IsActive)
            .OrderBy(uta => uta.Tenant!.CompanyName ?? uta.Tenant.Name)
            .ToListAsync(ct);

        var dtos = tenantAccessList.Select(uta => uta.ToDto()).ToList();
        return Result<List<UserTenantAccessDto>>.Ok(dtos);
    }
}
