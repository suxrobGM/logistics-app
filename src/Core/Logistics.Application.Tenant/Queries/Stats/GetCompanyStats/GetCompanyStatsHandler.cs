using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetCompanyStatsHandler : RequestHandler<GetCompanyStatsQuery, ResponseResult<CompanyStatsDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetCompanyStatsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<CompanyStatsDto>> HandleValidated(
        GetCompanyStatsQuery req, CancellationToken cancellationToken)
    {
        var companyStats = new CompanyStatsDto();

        var rolesDict = await _tenantRepository.GetDictionaryAsync<string, TenantRole>(i => i.Name);
        var ownerRoleId = rolesDict[TenantRoles.Owner].Id;
        var managerRoleId = rolesDict[TenantRoles.Manager].Id;
        var dispatcherRoleId = rolesDict[TenantRoles.Dispatcher].Id;
        var driverRoleId = rolesDict[TenantRoles.Driver].Id;

        var ownerEmployee = await _tenantRepository.GetAsync<Employee>(e => e.EmployeeRoles.Any(er => er.RoleId == ownerRoleId));

        companyStats.OwnerName = ownerEmployee?.GetFullName();
        companyStats.EmployeesCount = await _tenantRepository.CountAsync<Employee>();
        companyStats.ManagersCount = await _tenantRepository.CountAsync<Employee>(e => e.EmployeeRoles.Any(er => er.RoleId == managerRoleId));
        companyStats.DispatchersCount = await _tenantRepository.CountAsync<Employee>(e => e.EmployeeRoles.Any(er => er.RoleId == dispatcherRoleId));
        companyStats.DriversCount = await _tenantRepository.CountAsync<Employee>(e => e.EmployeeRoles.Any(er => er.RoleId == driverRoleId));
        companyStats.TrucksCount = await _tenantRepository.CountAsync<Truck>();

        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var lastWeekStart = startOfWeek.AddDays(-7);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = startOfMonth.AddMonths(-1);

        var loads = _tenantRepository.Query<Load>();
        var thisWeekLoads = loads.Where(l => l.DeliveryDate >= startOfWeek);
        var lastWeekLoads = loads.Where(l => l.DeliveryDate >= lastWeekStart && l.DeliveryDate < startOfWeek);
        var thisMonthLoads = loads.Where(l => l.DeliveryDate >= startOfMonth);
        var lastMonthLoads = loads.Where(l => l.DeliveryDate >= lastMonthStart && l.DeliveryDate < startOfMonth);

        companyStats.ThisWeekGross = thisWeekLoads.Sum(l => l.DeliveryCost);
        companyStats.ThisWeekDistance = thisWeekLoads.Sum(l => l.Distance);
        companyStats.LastWeekGross = lastWeekLoads.Sum(l => l.DeliveryCost);
        companyStats.LastWeekDistance = lastWeekLoads.Sum(l => l.Distance);
        companyStats.ThisMonthGross = thisMonthLoads.Sum(l => l.DeliveryCost);
        companyStats.ThisMonthDistance = thisMonthLoads.Sum(l => l.Distance);
        companyStats.LastMonthGross = lastMonthLoads.Sum(l => l.DeliveryCost);
        companyStats.LastMonthDistance = lastMonthLoads.Sum(l => l.Distance);

        var totalStats = loads
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
