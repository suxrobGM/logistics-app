using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Shared.Consts;

namespace Logistics.Application.Queries;

internal sealed class GetDriverStatsHandler : RequestHandler<GetDriverStatsQuery, Result<DriverStatsDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetDriverStatsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<DriverStatsDto>> HandleValidated(
        GetDriverStatsQuery req, CancellationToken cancellationToken)
    {
        var driverStats = new DriverStatsDto();
        var driver = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (driver is null)
        {
            return Result<DriverStatsDto>.Fail($"Could not find driver with the user ID '{req.UserId}'");
        }

        var assignedTruck = await _tenantUow.Repository<Truck>().GetAsync(i => i.MainDriverId == driver.Id
                                                                               || i.SecondaryDriverId == driver.Id);

        if (assignedTruck is null)
        {
            return Result<DriverStatsDto>.Fail("Driver does not have an assigned truck");
        }

        var driverIncomePercentage = driver.SalaryType == SalaryType.ShareOfGross ? driver.Salary : 0;
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var lastWeekStart = startOfWeek.AddDays(-7);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = startOfMonth.AddMonths(-1);
        
        var loadsRepository = _tenantUow.Repository<Load>();
        var thisWeekLoads = loadsRepository.ApplySpecification(new FilterLoadsByDeliveryDate(assignedTruck.Id, startOfWeek, now));
        var lastWeekLoads = loadsRepository.ApplySpecification(new FilterLoadsByDeliveryDate(assignedTruck.Id, lastWeekStart, startOfWeek));
        var thisMonthLoads = loadsRepository.ApplySpecification(new FilterLoadsByDeliveryDate(assignedTruck.Id, startOfMonth, now));
        var lastMonthLoads = loadsRepository.ApplySpecification(new FilterLoadsByDeliveryDate(assignedTruck.Id, lastMonthStart, startOfMonth));

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
        return Result<DriverStatsDto>.Succeed(driverStats);
    }
}
