using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckByIdHandler : RequestHandler<GetTruckByIdQuery, ResponseResult<TruckDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTruckByIdHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<TruckDto>> HandleValidated(
        GetTruckByIdQuery req, CancellationToken cancellationToken)
    {
        var truckEntity = await _tenantRepository.GetAsync<Truck>(req.Id);
        
        if (truckEntity == null)
            return ResponseResult<TruckDto>.CreateError("Could not find the specified truck");
        
        var truckDto = truckEntity.ToDto();

        if (req.IncludeLoadIds)
        {
            var loadIds = _tenantRepository.Query<Truck>()
                .SelectMany(i => i.Loads)
                .Select(i => i.Id)
                .ToArray();

            truckDto.LoadIds = loadIds;
        }

        return ResponseResult<TruckDto>.CreateSuccess(truckDto);
    }
}
