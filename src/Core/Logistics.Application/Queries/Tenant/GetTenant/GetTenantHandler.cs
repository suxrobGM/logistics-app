using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTenantHandler : IAppRequestHandler<GetTenantQuery, Result<TenantDto>>
{
    private readonly IMasterUnitOfWork _masterUow;

    public GetTenantHandler(IMasterUnitOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    public async Task<Result<TenantDto>> Handle(
        GetTenantQuery req, CancellationToken ct)
    {
        var tenantEntity = await _masterUow.Repository<Tenant>().GetAsync(i => i.Id == req.Id || i.Name == req.Name);

        if (tenantEntity is null)
        {
            return Result<TenantDto>.Fail($"Could not find a tenant with ID or Name: {req.Id} or {req.Name}");
        }

        // Count the number of employees belonging to the tenant
        var employeeCount = await _masterUow.Repository<User>().CountAsync(i => i.TenantId == tenantEntity.Id);

        var tenantDto = tenantEntity.ToDto(req.IncludeConnectionString, employeeCount);
        return Result<TenantDto>.Ok(tenantDto);
    }
}
