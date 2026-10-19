using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Queries;

internal sealed class GetTenantsHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetTenantsQuery, PagedResult<TenantDto>>
{
    public Task<PagedResult<TenantDto>> Handle(
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

        var totalItems = query.Count();

        var items = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto(req.IncludeConnectionStrings, null))
            .ToArray();

        return Task.FromResult(PagedResult<TenantDto>.Ok(items, totalItems, req.PageSize));
    }
}
