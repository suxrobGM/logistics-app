using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDriverStatsHandler : RequestHandler<GetDriverStatsQuery, ResponseResult<DriverStatsDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetDriverStatsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<DriverStatsDto>> HandleValidated(
        GetDriverStatsQuery req, CancellationToken cancellationToken)
    {
        var driverStats = new DriverStatsDto();

        var driver = await _tenantRepository.GetAsync<Employee>(req.UserId);

        if (driver is null)
        {
            return ResponseResult<DriverStatsDto>.CreateError($"Could not find driver with the user ID '{req.UserId}'");
        }

        if (string.IsNullOrEmpty(driver.TruckId))
        {
            return ResponseResult<DriverStatsDto>.CreateError("Driver does not have an assigned truck");
        }

        var driverIncomePercentage = (decimal)driver.Truck!.DriverIncomePercentage;
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var lastWeekStart = startOfWeek.AddDays(-7);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = startOfMonth.AddMonths(-1);

        var loads = _tenantRepository.Query<Load>();
        var thisWeekLoads = loads.Where(l => l.AssignedTruckId == driver.TruckId && l.DeliveryDate >= startOfWeek);
        var lastWeekLoads = loads.Where(l => l.AssignedTruckId == driver.TruckId && l.DeliveryDate >= lastWeekStart && l.DeliveryDate < startOfWeek);
        var thisMonthLoads = loads.Where(l => l.AssignedTruckId == driver.TruckId && l.DeliveryDate >= startOfMonth);
        var lastMonthLoads = loads.Where(l => l.AssignedTruckId == driver.TruckId && l.DeliveryDate >= lastMonthStart && l.DeliveryDate < startOfMonth);

        driverStats.ThisWeekGross = thisWeekLoads.Sum(l => l.DeliveryCost);
        driverStats.ThisWeekShare = driverStats.ThisWeekGross * driverIncomePercentage;
        driverStats.ThisWeekDistance = thisWeekLoads.Sum(l => l.Distance);
        driverStats.LastWeekGross = lastWeekLoads.Sum(l => l.DeliveryCost);
        driverStats.LastWeekShare = driverStats.LastWeekGross * driverIncomePercentage;
        driverStats.LastWeekDistance = lastWeekLoads.Sum(l => l.Distance);
        driverStats.ThisMonthGross = thisMonthLoads.Sum(l => l.DeliveryCost);
        driverStats.ThisMonthShare = driverStats.ThisMonthGross * driverIncomePercentage;
        driverStats.ThisMonthDistance = thisMonthLoads.Sum(l => l.Distance);
        driverStats.LastMonthGross = lastMonthLoads.Sum(l => l.DeliveryCost);
        driverStats.LastMonthShare = driverStats.LastMonthGross * driverIncomePercentage;
        driverStats.LastMonthDistance = lastMonthLoads.Sum(l => l.Distance);
        return ResponseResult<DriverStatsDto>.CreateSuccess(driverStats);
    }
}
