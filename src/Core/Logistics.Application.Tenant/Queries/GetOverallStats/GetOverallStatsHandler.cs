using Logistics.Domain.Enums;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetOverallStatsHandler : RequestHandler<GetOverallStatsQuery, ResponseResult<OverallStatsDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetOverallStatsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override async Task<ResponseResult<OverallStatsDto>> HandleValidated(
        GetOverallStatsQuery req, CancellationToken cancellationToken)
    {
        var overallStatsDto = new OverallStatsDto();
        var rolesDict = await _tenantRepository.GetDictionaryAsync<string, TenantRole>(i => i.Name);
        var employees = await _tenantRepository.GetListAsync<Employee>();
        
        var ownerRole = rolesDict[TenantRoles.Owner];
        var managerRole = rolesDict[TenantRoles.Manager];
        var dispatcherRole = rolesDict[TenantRoles.Dispatcher];
        var driverRole = rolesDict[TenantRoles.Driver];
        
        var ownerEmployee = employees.FirstOrDefault(i => i.Roles.Contains(ownerRole));
        overallStatsDto.OwnerName = ownerEmployee?.GetFullName();

        var employeesCount = employees.Count;
        var managersCount = employees.Count(i => i.Roles.Contains(managerRole));
        var dispatchersCount = employees.Count(i => i.Roles.Contains(dispatcherRole));
        var driversCount = employees.Count(i => i.Roles.Contains(driverRole));
        
        var sum = _tenantRepository
            .Query<Load>()
            .GroupBy(_ => 1)
            .Select(i => new
            {
                TotalDistance = i.Sum(m => m.Distance),
                TotalGross = i.Sum(m => m.DeliveryCost)
            })
            .FirstOrDefault();

        overallStatsDto.EmployeesCount = employeesCount;
        overallStatsDto.ManagersCount = managersCount;
        overallStatsDto.DispatchersCount = dispatchersCount;
        overallStatsDto.DriversCount = driversCount;
        overallStatsDto.TotalDistance = sum?.TotalDistance ?? 0;
        overallStatsDto.TotalIncome = sum?.TotalGross ?? 0;
        return ResponseResult<OverallStatsDto>.CreateSuccess(overallStatsDto);
    }
}
