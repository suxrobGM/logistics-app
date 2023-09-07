using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDriverDashboardDataHandler : RequestHandler<GetDriverDashboardDataQuery, ResponseResult<DriverDashboardDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetDriverDashboardDataHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override async Task<ResponseResult<DriverDashboardDto>> HandleValidated(
        GetDriverDashboardDataQuery req, CancellationToken cancellationToken)
    {
        var driver = await _tenantRepository.GetAsync<Employee>(req.UserId);
        
        if (driver == null)
            return ResponseResult<DriverDashboardDto>.CreateError($"Could not find a driver with ID '{req.UserId}'");
        
        var activeLoads = _tenantRepository.ApplySpecification(new GetDriverActiveLoads(req.UserId))
            .Select(i => i.ToDto())
            .ToArray();

        var driverDashboardDto = new DriverDashboardDto()
        {
            TruckNumber = driver.Truck?.TruckNumber,
            DriverFullName = driver.GetFullName(),
            ActiveLoads = activeLoads!
        };

        return ResponseResult<DriverDashboardDto>.CreateSuccess(driverDashboardDto);
    }
}
