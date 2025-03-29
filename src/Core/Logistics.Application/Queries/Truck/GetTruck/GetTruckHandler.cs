using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTruckHandler : RequestHandler<GetTruckQuery, Result<TruckDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTruckHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<TruckDto>> HandleValidated(
        GetTruckQuery req, CancellationToken cancellationToken)
    {
        var truckEntity = await TryGetTruck(req.TruckOrDriverId);

        if (truckEntity is null)
        {
            return Result<TruckDto>.Fail($"Could not find a truck with ID '{req.TruckOrDriverId}'");
        }

        var truckDto = ConvertToDto(truckEntity, req.IncludeLoads, req.OnlyActiveLoads);
        return Result<TruckDto>.Succeed(truckDto);
    }

    private async Task<Truck?> TryGetTruck(string? truckOrDriverId)
    {
        if (string.IsNullOrEmpty(truckOrDriverId))
            return null;

        var truckRepository = _tenantUow.Repository<Truck>();
        var truck = await truckRepository.GetAsync(i => i.Id == truckOrDriverId);
        return truck ?? await GetTruckFromDriver(truckOrDriverId);
    }

    private async Task<Truck?> GetTruckFromDriver(string userId)
    {
        var employeeRepository = _tenantUow.Repository<Employee>();
        var driver = await employeeRepository.GetByIdAsync(userId);
        return driver?.Truck;
    }

    private static TruckDto ConvertToDto(Truck truckEntity, bool includeLoads, bool onlyActiveLoads)
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
