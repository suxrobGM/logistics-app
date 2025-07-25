﻿using Logistics.Application.Specifications;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTenantsHandler : RequestHandler<GetTenantsQuery, PagedResult<TenantDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetTenantsHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResult<TenantDto>> HandleValidated(
        GetTenantsQuery req, CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<Domain.Entities.Tenant>().CountAsync();
        var spec = new SearchTenants(req.Search, req.OrderBy, req.Page, req.PageSize);

        var items = _masterUow.Repository<Domain.Entities.Tenant>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto(req.IncludeConnectionStrings, null))
            .ToArray();
        
        return PagedResult<TenantDto>.Succeed(items, totalItems, req.PageSize);
    }
}
