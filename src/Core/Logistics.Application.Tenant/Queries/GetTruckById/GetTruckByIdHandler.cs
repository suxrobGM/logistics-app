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
        var truckEntity = await _tenantRepository.GetAsync<Truck>(i => i.Id == req.Id);

        if (truckEntity == null)
        {
            return ResponseResult<TruckDto>.CreateError("Could not find the specified truck");
        }

        var truckDto = truckEntity.ToDto(new List<LoadDto>());

        if (req.IncludeLoads)
        {
            truckDto.Loads = truckEntity.Loads
                .Select(l => l.ToDto())
                .ToArray();
        }

        return ResponseResult<TruckDto>.CreateSuccess(truckDto);
    }
}
