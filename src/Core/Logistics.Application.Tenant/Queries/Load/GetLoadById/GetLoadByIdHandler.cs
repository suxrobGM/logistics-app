using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetLoadByIdHandler : RequestHandler<GetLoadByIdQuery, ResponseResult<LoadDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetLoadByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult<LoadDto>> HandleValidated(
        GetLoadByIdQuery req, CancellationToken cancellationToken)
    {
        var loadEntity = await _tenantUow.Repository<Load>().GetByIdAsync(req.Id);

        if (loadEntity is null)
        {
            return ResponseResult<LoadDto>.CreateError($"Could not find a load with ID '{req.Id}'");
        }
        
        var loadDto = loadEntity.ToDto();
        return ResponseResult<LoadDto>.CreateSuccess(loadDto);
    }
}
