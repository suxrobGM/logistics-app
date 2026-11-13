using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Queries;

internal sealed class GetTenantsHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetTenantsQuery, PagedResult<TenantDto>>
{
    public async Task<PagedResult<TenantDto>> Handle(
        GetTenantsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<Tenant>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i =>
                (!string.IsNullOrEmpty(i.Name) && i.Name.ToLower().Contains(search)) ||
                (!string.IsNullOrEmpty(i.CompanyName) && i.CompanyName.ToLower().Contains(search)));
        }

        return await query.ToPagedResultAsync(req, i => i.ToDto(null), ct);
    }
}
