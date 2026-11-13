using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Loads;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Stats.Queries;

internal sealed class GetDriverStatsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetDriverStatsQuery, Result<DriverStatsDto>>
{
    public async Task<Result<DriverStatsDto>> Handle(
        GetDriverStatsQuery req, CancellationToken ct)
    {
        var driverStats = new DriverStatsDto();
        var driver = await tenantUow.Repository<Employee>().GetByIdAsync(req.UserId, ct);

        if (driver is null)
        {
            return Result<DriverStatsDto>.Fail($"Could not find driver with the user ID '{req.UserId}'", ErrorCodes.NotFound);
        }

        var assignedTruck = await tenantUow.Repository<Truck>().GetAsync(i => i.MainDriverId == driver.Id
                                                                               || i.SecondaryDriverId == driver.Id, ct);

        if (assignedTruck is null)
        {
            return Result<DriverStatsDto>.Fail("Driver does not have an assigned truck");
        }

        var driverIncomePercentage = driver.SalaryType == SalaryType.ShareOfGross ? driver.Salary.Amount : 0;
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var lastWeekStart = startOfWeek.AddDays(-7);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = startOfMonth.AddMonths(-1);

        // startOfWeek lands after now on a Sunday, so the range end is the latest bound, not now.
        var rangeStart = lastWeekStart < lastMonthStart ? lastWeekStart : lastMonthStart;
        var rangeEnd = new[] { now, startOfWeek, startOfMonth }.Max();
        var loads = await tenantUow.Repository<Load>().Query()
            .FilterDeliveredBetween(assignedTruck.Id, rangeStart, rangeEnd)
            .ToListAsync(ct);

        var thisWeekLoads = DeliveredBetween(loads, startOfWeek, now);
        var lastWeekLoads = DeliveredBetween(loads, lastWeekStart, startOfWeek);
        var thisMonthLoads = DeliveredBetween(loads, startOfMonth, now);
        var lastMonthLoads = DeliveredBetween(loads, lastMonthStart, startOfMonth);

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
        return Result<DriverStatsDto>.Ok(driverStats);
    }

    /// <summary>Same inclusive bounds as FilterDeliveredBetween.</summary>
    private static List<Load> DeliveredBetween(List<Load> loads, DateTime start, DateTime end) =>
        loads.Where(l => l.DeliveredAt >= start && l.DeliveredAt <= end).ToList();
}
