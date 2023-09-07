using Logistics.Application.Tenant.Mappers;
using Logistics.Models;
using LoadStatus = Logistics.Domain.Enums.LoadStatus;

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
        GetDriverDashboardDataQuery request, CancellationToken cancellationToken)
    {
        var driver = await _mainRepository.GetAsync<User>(i => i.Id == request.UserId);

        if (driver == null)
            return ResponseResult<DriverDashboardDto>.CreateError($"Could not find a driver with ID '{request.UserId}'");

        var activeLoad = await _tenantRepository.GetAsync<Load>(i => i.AssignedDriverId == driver.Id && i.Status != LoadStatus.Delivered);
        var truck = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == driver.Id);

        var driverDashboardDto = new DriverDashboardDto()
        {
            TruckNumber = truck?.TruckNumber,
            DriverFullName = driver.GetFullName(),
            AssignedLoad = LoadMapper.ToDto(activeLoad)
        };

        return ResponseResult<DriverDashboardDto>.CreateSuccess(driverDashboardDto);
    }
}
