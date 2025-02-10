using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTenantHandler : RequestHandler<GetTenantQuery, Result<TenantDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetTenantHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result<TenantDto>> HandleValidated(
        GetTenantQuery req, CancellationToken cancellationToken)
    {
        var tenantEntity = await _masterUow.Repository<Domain.Entities.Tenant>().GetAsync(i => i.Id == req.Id || i.Name == req.Name);

        if (tenantEntity is null)
        {
            return Result<TenantDto>.Fail($"Could not find a tenant with ID or Name: {req.Id} or {req.Name}");
        }

        var tenantDto = tenantEntity.ToDto(req.IncludeConnectionString);
        return Result<TenantDto>.Succeed(tenantDto);
    }
}
