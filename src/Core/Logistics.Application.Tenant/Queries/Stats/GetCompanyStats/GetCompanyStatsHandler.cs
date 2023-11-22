using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetCompanyStatsHandler : RequestHandler<GetCompanyStatsQuery, ResponseResult<CompanyStatsDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetCompanyStatsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult<CompanyStatsDto>> HandleValidated(
        GetCompanyStatsQuery req, CancellationToken cancellationToken)
    {
        var companyStats = new CompanyStatsDto();

        var rolesDict = (await _tenantUow.Repository<TenantRole>().GetListAsync()).ToDictionary(i => i.Name);
        var ownerRoleId = rolesDict[TenantRoles.Owner].Id;
        var managerRoleId = rolesDict[TenantRoles.Manager].Id;
        var dispatcherRoleId = rolesDict[TenantRoles.Dispatcher].Id;
        var driverRoleId = rolesDict[TenantRoles.Driver].Id;

        var employeeRepository = _tenantUow.Repository<Employee>();
        var ownerEmployee = await employeeRepository.GetAsync(e => e.EmployeeRoles.Any(er => er.RoleId == ownerRoleId));

        companyStats.OwnerName = ownerEmployee?.GetFullName();
        companyStats.EmployeesCount = await employeeRepository.CountAsync();
        companyStats.ManagersCount = await employeeRepository.CountAsync(e => e.EmployeeRoles.Any(er => er.RoleId == managerRoleId));
        companyStats.DispatchersCount = await employeeRepository.CountAsync(e => e.EmployeeRoles.Any(er => er.RoleId == dispatcherRoleId));
        companyStats.DriversCount = await employeeRepository.CountAsync(e => e.EmployeeRoles.Any(er => er.RoleId == driverRoleId));
        companyStats.TrucksCount = await _tenantUow.Repository<Truck>().CountAsync();

        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var lastWeekStart = startOfWeek.AddDays(-7);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = startOfMonth.AddMonths(-1);
        
        var loadsRepository = _tenantUow.Repository<Load>();
        var thisWeekLoads = loadsRepository.ApplySpecification(new FilterLoadsByDeliveryDate(null, startOfWeek, now));
        var lastWeekLoads = loadsRepository.ApplySpecification(new FilterLoadsByDeliveryDate(null, lastWeekStart, startOfWeek));
        var thisMonthLoads = loadsRepository.ApplySpecification(new FilterLoadsByDeliveryDate(null, startOfMonth, now));
        var lastMonthLoads = loadsRepository.ApplySpecification(new FilterLoadsByDeliveryDate(null, lastMonthStart, startOfMonth));

        companyStats.ThisWeekGross = thisWeekLoads.Sum(l => l.DeliveryCost);
        companyStats.ThisWeekDistance = thisWeekLoads.Sum(l => l.Distance);
        companyStats.LastWeekGross = lastWeekLoads.Sum(l => l.DeliveryCost);
        companyStats.LastWeekDistance = lastWeekLoads.Sum(l => l.Distance);
        companyStats.ThisMonthGross = thisMonthLoads.Sum(l => l.DeliveryCost);
        companyStats.ThisMonthDistance = thisMonthLoads.Sum(l => l.Distance);
        companyStats.LastMonthGross = lastMonthLoads.Sum(l => l.DeliveryCost);
        companyStats.LastMonthDistance = lastMonthLoads.Sum(l => l.Distance);

        var totalStats = loadsRepository.Query()
            .GroupBy(_ => 1).
            Select(i => new
            {
                TotalDistance = i.Sum(m => m.Distance),
                TotalGross = i.Sum(m => m.DeliveryCost)
            }).FirstOrDefault();

        companyStats.TotalDistance = totalStats?.TotalDistance ?? 0;
        companyStats.TotalGross = totalStats?.TotalGross ?? 0;
        return ResponseResult<CompanyStatsDto>.CreateSuccess(companyStats);
    }
}
