using Logistics.Application.Tenant.Mappers;
using Logistics.Domain.Enums;
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
        var driver = await _tenantRepository.GetAsync<Employee>(req.UserId);
        
        if (driver == null)
            return ResponseResult<TruckDto>.CreateError($"Could not find the specified driver with ID {req.UserId}");

        if (driver.Truck == null)
            return ResponseResult<TruckDto>.CreateError("The driver is not associated with any truck");
        
        var truckDto = driver.Truck.ToDto(new List<LoadDto>());
        
        if (req.IncludeOnlyActiveLoads)
        {
            truckDto.Loads = driver.Truck.Loads
                .Where(l => l.DeliveryDate == null)
                .Select(l => l.ToDto())
                .ToArray();
        }
        else if (req.IncludeLoads)
        {
            truckDto.Loads = driver.Truck.Loads
                .Select(l => l.ToDto())
                .ToArray();
        }
        
        return ResponseResult<TruckDto>.CreateSuccess(truckDto);
    }
}
