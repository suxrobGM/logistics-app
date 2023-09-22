using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDriverActiveLoadsHandler : RequestHandler<GetDriverActiveLoadsQuery, ResponseResult<DriverActiveLoadsDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetDriverActiveLoadsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override async Task<ResponseResult<DriverActiveLoadsDto>> HandleValidated(
        GetDriverActiveLoadsQuery req, CancellationToken cancellationToken)
    {
        var driver = await _tenantRepository.GetAsync<Employee>(req.UserId);
        
        if (driver == null)
            return ResponseResult<DriverActiveLoadsDto>.CreateError($"Could not find a driver with ID '{req.UserId}'");
        
        var activeLoads = _tenantRepository.ApplySpecification(new GetDriverActiveLoads(req.UserId!))
            .Select(i => i.ToDto());

        var driverDashboardDto = new DriverActiveLoadsDto
        {
            TruckNumber = driver.Truck?.TruckNumber,
            ActiveLoads = activeLoads
        };

        return ResponseResult<DriverActiveLoadsDto>.CreateSuccess(driverDashboardDto);
    }
}
