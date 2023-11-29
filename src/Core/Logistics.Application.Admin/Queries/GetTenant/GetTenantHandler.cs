using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetTenantHandler : RequestHandler<GetTenantQuery, ResponseResult<TenantDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetTenantHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<ResponseResult<TenantDto>> HandleValidated(
        GetTenantQuery req, CancellationToken cancellationToken)
    {
        var tenantEntity = await _masterUow.Repository<Tenant>().GetAsync(i => i.Id == req.Id || i.Name == req.Name);

        if (tenantEntity is null)
        {
            return ResponseResult<TenantDto>.CreateError("Could not find the specified tenant");
        }

        var tenantDto = tenantEntity.ToDto(req.IncludeConnectionString);
        return ResponseResult<TenantDto>.CreateSuccess(tenantDto);
    }
}
