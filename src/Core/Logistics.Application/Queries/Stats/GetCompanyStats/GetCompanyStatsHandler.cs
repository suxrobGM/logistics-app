using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetCompanyStatsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetCompanyStatsQuery, Result<CompanyStatsDto>>
{
    public async Task<Result<CompanyStatsDto>> Handle(
        GetCompanyStatsQuery req, CancellationToken ct)
    {
        var companyStats = new CompanyStatsDto();

        try
        {
            var result = await tenantUow.ExecuteRawSql<CompanyStatsDto>("SELECT * FROM get_company_stats();", ct);
            companyStats = result.FirstOrDefault() ?? new CompanyStatsDto();
        }
        catch
        {
            await PopulateStatsFromEfCore(companyStats, ct);
        }

        await PopulateDashboardKpis(companyStats, ct);
        return Result<CompanyStatsDto>.Ok(companyStats);
    }

    private async Task PopulateStatsFromEfCore(CompanyStatsDto stats, CancellationToken ct)
    {
        // Calculate all date boundaries once
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var lastWeekStart = startOfWeek.AddDays(-7);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = startOfMonth.AddMonths(-1);
        var threeMonthsAgo = now.AddMonths(-3);
        var oneYearAgo = now.AddYears(-1);

        // Single query for employee counts by role (instead of 5 separate queries)
        var rolesDict = (await tenantUow.Repository<TenantRole>().GetListAsync(ct: ct))
            .ToDictionary(i => i.Name, i => i.Id);

        var employeeCounts = await tenantUow.Repository<Employee>().Query()
            .Where(e => e.RoleId != null)
            .GroupBy(e => e.RoleId!.Value)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countsDict = employeeCounts.ToDictionary(x => x.RoleId, x => x.Count);

        stats.EmployeesCount = await tenantUow.Repository<Employee>().CountAsync(ct: ct);
        stats.ManagersCount = countsDict.GetValueOrDefault(rolesDict[TenantRoles.Manager]);
        stats.DispatchersCount = countsDict.GetValueOrDefault(rolesDict[TenantRoles.Dispatcher]);
        stats.DriversCount = countsDict.GetValueOrDefault(rolesDict[TenantRoles.Driver]);

        // Get owner name
        var ownerRoleId = rolesDict[TenantRoles.Owner];
        var ownerEmployee = await tenantUow.Repository<Employee>()
            .GetAsync(e => e.RoleId == ownerRoleId, ct);
        stats.OwnerName = ownerEmployee?.GetFullName();

        stats.TrucksCount = await tenantUow.Repository<Truck>().CountAsync(ct: ct);

        // Single query for all load stats using conditional aggregation
        var loadStats = await tenantUow.Repository<Load>().Query()
            .Where(l => l.DeliveredAt != null)
            .GroupBy(_ => 1)
            .Select(_ => new
            {
                // This week
                ThisWeekGross = _.Where(l => l.DeliveredAt >= startOfWeek && l.DeliveredAt < now)
                    .Sum(l => l.DeliveryCost.Amount),
                ThisWeekDistance = _.Where(l => l.DeliveredAt >= startOfWeek && l.DeliveredAt < now)
                    .Sum(l => l.Distance),
                // Last week
                LastWeekGross = _.Where(l => l.DeliveredAt >= lastWeekStart && l.DeliveredAt < startOfWeek)
                    .Sum(l => l.DeliveryCost.Amount),
                LastWeekDistance = _.Where(l => l.DeliveredAt >= lastWeekStart && l.DeliveredAt < startOfWeek)
                    .Sum(l => l.Distance),
                // This month
                ThisMonthGross = _.Where(l => l.DeliveredAt >= startOfMonth && l.DeliveredAt < now)
                    .Sum(l => l.DeliveryCost.Amount),
                ThisMonthDistance = _.Where(l => l.DeliveredAt >= startOfMonth && l.DeliveredAt < now)
                    .Sum(l => l.Distance),
                // Last month
                LastMonthGross = _.Where(l => l.DeliveredAt >= lastMonthStart && l.DeliveredAt < startOfMonth)
                    .Sum(l => l.DeliveryCost.Amount),
                LastMonthDistance = _.Where(l => l.DeliveredAt >= lastMonthStart && l.DeliveredAt < startOfMonth)
                    .Sum(l => l.Distance),
                // Last 3 months
                LastThreeMonthsGross = _.Where(l => l.DeliveredAt >= threeMonthsAgo && l.DeliveredAt < now)
                    .Sum(l => l.DeliveryCost.Amount),
                LastThreeMonthsDistance = _.Where(l => l.DeliveredAt >= threeMonthsAgo && l.DeliveredAt < now)
                    .Sum(l => l.Distance),
                // Last year
                LastYearGross = _.Where(l => l.DeliveredAt >= oneYearAgo && l.DeliveredAt < now)
                    .Sum(l => l.DeliveryCost.Amount),
                LastYearDistance = _.Where(l => l.DeliveredAt >= oneYearAgo && l.DeliveredAt < now)
                    .Sum(l => l.Distance),
                // Total
                TotalGross = _.Sum(l => l.DeliveryCost.Amount),
                TotalDistance = _.Sum(l => l.Distance)
            })
            .FirstOrDefaultAsync(ct);

        if (loadStats is not null)
        {
            stats.ThisWeekGross = loadStats.ThisWeekGross;
            stats.ThisWeekDistance = loadStats.ThisWeekDistance;
            stats.LastWeekGross = loadStats.LastWeekGross;
            stats.LastWeekDistance = loadStats.LastWeekDistance;
            stats.ThisMonthGross = loadStats.ThisMonthGross;
            stats.ThisMonthDistance = loadStats.ThisMonthDistance;
            stats.LastMonthGross = loadStats.LastMonthGross;
            stats.LastMonthDistance = loadStats.LastMonthDistance;
            stats.LastThreeMonthsGross = loadStats.LastThreeMonthsGross;
            stats.LastThreeMonthsDistance = loadStats.LastThreeMonthsDistance;
            stats.LastYearGross = loadStats.LastYearGross;
            stats.LastYearDistance = loadStats.LastYearDistance;
            stats.TotalGross = loadStats.TotalGross;
            stats.TotalDistance = loadStats.TotalDistance;
        }
    }

    private async Task PopulateDashboardKpis(CompanyStatsDto stats, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var loadsRepository = tenantUow.Repository<Load>();
        var truckRepository = tenantUow.Repository<Truck>();

        // Single query for load counts by status (instead of 2 separate CountAsync calls)
        var loadCountsByStatus = await loadsRepository.Query()
            .GroupBy(_ => 1)
            .Select(_ => new
            {
                ActiveCount = _.Count(l => l.Status == LoadStatus.Dispatched || l.Status == LoadStatus.PickedUp),
                UnassignedCount = _.Count(l => l.Status == LoadStatus.Draft && l.AssignedTruckId == null)
            })
            .FirstOrDefaultAsync(ct);

        stats.ActiveLoadsCount = loadCountsByStatus?.ActiveCount ?? 0;
        stats.UnassignedLoadsCount = loadCountsByStatus?.UnassignedCount ?? 0;

        // Idle trucks calculation
        var activeTruckCount = await loadsRepository.Query()
            .Where(l => l.Status == LoadStatus.Dispatched || l.Status == LoadStatus.PickedUp)
            .Where(l => l.AssignedTruckId != null)
            .Select(l => l.AssignedTruckId!.Value)
            .Distinct()
            .CountAsync(ct);

        // Use cached TrucksCount if available, otherwise query
        var totalTrucks = stats.TrucksCount > 0
            ? stats.TrucksCount
            : await truckRepository.CountAsync(ct: ct);
        stats.IdleTrucksCount = totalTrucks - activeTruckCount;

        // Financial health - single query for invoice stats
        // Overdue is calculated dynamically: invoices with status Issued/PartiallyPaid where DueDate < now
        var invoiceRepository = tenantUow.Repository<LoadInvoice>();
        var outstandingStatuses = new[]
        {
            InvoiceStatus.Issued, InvoiceStatus.Sent, InvoiceStatus.PartiallyPaid
        };

        var invoiceStats = await invoiceRepository.Query()
            .Where(i => outstandingStatuses.Contains(i.Status))
            .GroupBy(_ => 1)
            .Select(_ => new
            {
                OutstandingTotal = _.Sum(i => i.Total.Amount - i.Payments.Sum(p => p.Amount.Amount)),
                OverdueCount = _.Count(i =>
                    (i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid) &&
                    i.DueDate.HasValue && i.DueDate < now)
            })
            .FirstOrDefaultAsync(ct);

        stats.OutstandingInvoiceTotal = invoiceStats?.OutstandingTotal ?? 0;
        stats.OverdueInvoiceCount = invoiceStats?.OverdueCount ?? 0;

        // Payments received this week
        stats.PaymentsReceivedThisWeek = await tenantUow.Repository<Payment>().Query()
            .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= startOfWeek)
            .SumAsync(p => p.Amount.Amount, ct);

        // Top 3 trucks by revenue this month - single query with join
        stats.TopTrucks = await loadsRepository.Query()
            .Where(l => l.DeliveredAt >= startOfMonth && l.AssignedTruck != null)
            .GroupBy(l => new
            {
                l.AssignedTruckId,
                l.AssignedTruck!.Number,
                DriverName = l.AssignedTruck.MainDriver != null
                    ? l.AssignedTruck.MainDriver.FirstName + " " + l.AssignedTruck.MainDriver.LastName
                    : null
            })
            .Select(g => new TopTruckDto
            {
                TruckId = g.Key.AssignedTruckId!.Value,
                TruckNumber = g.Key.Number,
                DriverName = g.Key.DriverName,
                Revenue = g.Sum(l => l.DeliveryCost.Amount)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(3)
            .ToListAsync(ct);
    }
}
