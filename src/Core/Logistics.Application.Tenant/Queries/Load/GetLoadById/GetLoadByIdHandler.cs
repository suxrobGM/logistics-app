using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetLoadByIdHandler : RequestHandler<GetLoadByIdQuery, ResponseResult<LoadDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetLoadByIdHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<LoadDto>> HandleValidated(
        GetLoadByIdQuery req, CancellationToken cancellationToken)
    {
        var loadEntity = await _tenantRepository.GetAsync<Load>(req.Id);

        if (loadEntity == null)
            return ResponseResult<LoadDto>.CreateError("Could not find the specified cargo");
        
        var loadDto = loadEntity.ToDto();
        return ResponseResult<LoadDto>.CreateSuccess(loadDto);
    }
}
