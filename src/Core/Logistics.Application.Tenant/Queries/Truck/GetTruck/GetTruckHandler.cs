using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckHandler : RequestHandler<GetTruckQuery, ResponseResult<TruckDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<TruckDto>> HandleValidated(
        GetTruckQuery req, CancellationToken cancellationToken)
    {
        var truckEntity = await TryGetTruck(req.TruckOrDriverId);

        if (truckEntity is null)
        {
            return ResponseResult<TruckDto>.CreateError($"Could not find a truck with ID '{req.TruckOrDriverId}'");
        }

        var truckDto = ConvertToDto(truckEntity, req.IncludeLoads, req.OnlyActiveLoads);
        return ResponseResult<TruckDto>.CreateSuccess(truckDto);
    }

    private async Task<Truck?> TryGetTruck(string? truckOrDriverId)
    {
        if (string.IsNullOrEmpty(truckOrDriverId))
            return default;

        var truck = await _tenantRepository.GetAsync<Truck>(i => i.Id == truckOrDriverId);
        return truck ?? await GetTruckFromDriver(truckOrDriverId);
    }

    private async Task<Truck?> GetTruckFromDriver(string userId)
    {
        var driver = await _tenantRepository.GetAsync<Employee>(userId);
        return driver?.Truck;
    }

    private TruckDto ConvertToDto(Truck truckEntity, bool includeLoads, bool onlyActiveLoads)
    {
        var truckDto = truckEntity.ToDto(new List<LoadDto>());

        if (!includeLoads) 
            return truckDto;
        
        var loads = truckEntity.Loads.Select(l => l.ToDto());
        if (onlyActiveLoads)
        {
            loads = loads.Where(l => l.DeliveryDate == null);
        }
        
        truckDto.Loads = loads.ToArray();
        return truckDto;
    }
}
