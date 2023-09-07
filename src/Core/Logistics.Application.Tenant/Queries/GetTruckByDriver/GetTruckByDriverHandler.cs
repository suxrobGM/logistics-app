using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckByDriverHandler : RequestHandler<GetTruckByDriverQuery, ResponseResult<TruckDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTruckByDriverHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<TruckDto>> HandleValidated(
        GetTruckByDriverQuery req, CancellationToken cancellationToken)
    {
        var driver = await _tenantRepository.GetAsync<Employee>(req.DriverId);
        
        if (driver == null)
            return ResponseResult<TruckDto>.CreateError($"Could not find the specified driver with ID {req.DriverId}");

        if (driver.Truck == null)
            return ResponseResult<TruckDto>.CreateError("The driver is not associated with any truck");
        
        var truckDto = driver.Truck.ToDto();
        
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
