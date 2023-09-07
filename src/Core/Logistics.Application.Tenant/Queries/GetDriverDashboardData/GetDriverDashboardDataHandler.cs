using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDriverDashboardDataHandler : RequestHandler<GetDriverDashboardDataQuery, ResponseResult<DriverDashboardDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMainRepository _mainRepository;

    public GetDriverDashboardDataHandler(
        ITenantRepository tenantRepository, 
        IMainRepository mainRepository)
    {
        _tenantRepository = tenantRepository;
        _mainRepository = mainRepository;
    }
    
    protected override async Task<ResponseResult<DriverDashboardDto>> HandleValidated(
        GetDriverDashboardDataQuery req, CancellationToken cancellationToken)
    {
        var user = await _mainRepository.GetAsync<User>(req.UserId);

        if (user == null)
            return ResponseResult<DriverDashboardDto>.CreateError($"Could not find a user with ID '{req.UserId}'");
        
        var driver = await _tenantRepository.GetAsync<Employee>(req.UserId);
        
        if (driver == null)
            return ResponseResult<DriverDashboardDto>.CreateError($"Could not find a driver with ID '{req.UserId}'");
        
        var activeLoads = _tenantRepository.ApplySpecification(new GetDriverActiveLoads(req.UserId))
            .Select(i => LoadMapper.ToDto(i))
            .ToArray();
        
        

        var driverDashboardDto = new DriverDashboardDto()
        {
            TruckNumber = driver.Truck?.TruckNumber,
            DriverFullName = user.GetFullName(),
            ActiveLoads = activeLoads!
        };

        return ResponseResult<DriverDashboardDto>.CreateSuccess(driverDashboardDto);
    }
}
