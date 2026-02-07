using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTenantHandler(IMasterUnitOfWork masterUow, ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetTenantQuery, Result<TenantDto>>
{
    public async Task<Result<TenantDto>> Handle(
        GetTenantQuery req, CancellationToken ct)
    {
        var tenantEntity = await masterUow.Repository<Tenant>().GetAsync(i => i.Id == req.Id || i.Name == req.Name, ct);

        if (tenantEntity is null)
        {
            return Result<TenantDto>.Fail($"Could not find a tenant with ID or Name: {req.Id} or {req.Name}");
        }

        // Count the number of trucks belonging to the tenant
        await tenantUow.SetCurrentTenantByIdAsync(tenantEntity.Id);
        var truckCount = await tenantUow.Repository<Truck>().CountAsync(ct: ct);

        var tenantDto = tenantEntity.ToDto(req.IncludeConnectionString, truckCount);
        return Result<TenantDto>.Ok(tenantDto);
    }
}
