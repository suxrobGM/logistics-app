using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class MaintenanceReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<MaintenanceReportQuery, Result<MaintenanceReportDto>>
{
    public async Task<Result<MaintenanceReportDto>> Handle(MaintenanceReportQuery req, CancellationToken ct)
    {
        var startDate = req.StartDate == default ? DateTime.UtcNow.AddMonths(-6) : req.StartDate;
        var endDate = req.EndDate == default ? DateTime.UtcNow : req.EndDate;
        var now = DateTime.UtcNow;
        var dueSoonThreshold = now.AddDays(30);

        var recordRepo = tenantUow.Repository<MaintenanceRecord>();
        var scheduleRepo = tenantUow.Repository<MaintenanceSchedule>();

        // Build base queries with filters
        var recordQuery = recordRepo.Query()
            .Where(r => r.ServiceDate >= startDate && r.ServiceDate <= endDate);
        var activeScheduleQuery = scheduleRepo.Query()
            .Where(s => s.IsActive);

        // Combined record summary (1 query instead of 6)
        var recordSummary = await recordQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalCost = g.Sum(r => r.TotalCost),
                LaborCost = g.Sum(r => r.LaborCost),
                PartsCost = g.Sum(r => r.PartsCost),
                TotalServices = g.Count(),
                ScheduledServices = g.Count(r => r.MaintenanceScheduleId.HasValue),
                UnscheduledServices = g.Count(r => !r.MaintenanceScheduleId.HasValue)
            })
            .FirstOrDefaultAsync(ct);

        var totalCost = recordSummary?.TotalCost ?? 0;
        var laborCost = recordSummary?.LaborCost ?? 0;
        var partsCost = recordSummary?.PartsCost ?? 0;
        var totalServices = recordSummary?.TotalServices ?? 0;
        var scheduledServices = recordSummary?.ScheduledServices ?? 0;
        var unscheduledServices = recordSummary?.UnscheduledServices ?? 0;

        // Combined schedule summary (1 query instead of 3)
        var scheduleSummary = await activeScheduleQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalSchedules = g.Count(),
                OverdueCount = g.Count(s => s.NextDueDate.HasValue && s.NextDueDate.Value < now),
                DueSoonCount = g.Count(s => s.NextDueDate.HasValue && s.NextDueDate.Value >= now && s.NextDueDate.Value <= dueSoonThreshold)
            })
            .FirstOrDefaultAsync(ct);

        var totalSchedules = scheduleSummary?.TotalSchedules ?? 0;
        var overdueCount = scheduleSummary?.OverdueCount ?? 0;
        var dueSoonCount = scheduleSummary?.DueSoonCount ?? 0;

        // Breakdowns
        var serviceTypeBreakdownData = await recordQuery
            .GroupBy(r => r.MaintenanceType)
            .Select(g => new
            {
                MaintenanceType = g.Key,
                Count = g.Count(),
                TotalCost = g.Sum(r => r.TotalCost)
            })
            .ToListAsync(ct);

        var vendorBreakdown = await recordQuery
            .Where(r => r.VendorName != null && r.VendorName != "")
            .GroupBy(r => r.VendorName)
            .Select(g => new
            {
                VendorName = g.Key,
                ServiceCount = g.Count(),
                TotalCost = g.Sum(r => r.TotalCost)
            })
            .OrderByDescending(v => v.TotalCost)
            .Take(10)
            .ToListAsync(ct);

        // Trends - group by year/month
        var trendsData = await recordQuery
            .GroupBy(r => new { r.ServiceDate.Year, r.ServiceDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                ServiceCount = g.Count(),
                TotalCost = g.Sum(r => r.TotalCost),
                LaborCost = g.Sum(r => r.LaborCost),
                PartsCost = g.Sum(r => r.PartsCost)
            })
            .ToListAsync(ct);

        // Top cost trucks
        var topTrucks = await recordQuery
            .GroupBy(r => r.TruckId)
            .Select(g => new
            {
                TruckId = g.Key,
                ServiceCount = g.Count(),
                TotalCost = g.Sum(r => r.TotalCost),
                LastServiceDate = g.Max(r => r.ServiceDate)
            })
            .OrderByDescending(t => t.TotalCost)
            .Take(5)
            .ToListAsync(ct);

        // Fetch truck numbers for top trucks
        var topTruckIds = topTrucks.Select(t => t.TruckId).ToList();
        var truckNumbers = await tenantUow.Repository<Truck>().Query()
            .Where(t => topTruckIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Number })
            .ToDictionaryAsync(t => t.Id, t => t.Number, ct);

        // Build service type breakdown with display names and calculated average
        var serviceTypeBreakdown = serviceTypeBreakdownData
            .Select(x => new MaintenanceTypeBreakdownDto
            {
                MaintenanceType = x.MaintenanceType.ToString(),
                MaintenanceTypeDisplay = x.MaintenanceType.GetDescription(),
                Count = x.Count,
                TotalCost = x.TotalCost,
                AverageCost = x.Count > 0 ? x.TotalCost / x.Count : 0
            })
            .OrderByDescending(t => t.TotalCost)
            .ToList();

        // Build trends with gaps filled in
        var trends = BuildTrends(trendsData, startDate, endDate);

        var dto = new MaintenanceReportDto
        {
            // Cost Summary
            TotalCost = totalCost,
            LaborCost = laborCost,
            PartsCost = partsCost,

            // Service Summary
            TotalServices = totalServices,
            ScheduledServices = scheduledServices,
            UnscheduledServices = unscheduledServices,

            // Schedule Summary
            TotalSchedules = totalSchedules,
            OverdueCount = overdueCount,
            DueSoonCount = dueSoonCount,

            // Breakdowns
            ByServiceType = serviceTypeBreakdown,
            ByVendor = vendorBreakdown
                .Select(x => new MaintenanceVendorBreakdownDto
                {
                    VendorName = x.VendorName ?? "Unknown",
                    ServiceCount = x.ServiceCount,
                    TotalCost = x.TotalCost
                })
                .ToList(),

            // Trends (same data for both cost and service trends)
            CostTrends = trends,
            ServiceTrends = trends,

            // Top cost trucks
            TopCostTrucks = topTrucks
                .Select(x => new MaintenanceTruckSummaryDto
                {
                    TruckId = x.TruckId,
                    TruckNumber = truckNumbers.GetValueOrDefault(x.TruckId, "Unknown"),
                    ServiceCount = x.ServiceCount,
                    TotalCost = x.TotalCost,
                    LastServiceDate = x.LastServiceDate
                })
                .ToList()
        };

        return Result<MaintenanceReportDto>.Ok(dto);
    }

    private static List<MaintenanceTrendDto> BuildTrends<T>(
        List<T> data,
        DateTime startDate,
        DateTime endDate) where T : class
    {
        // Build dictionary from database results
        var dataDict = new Dictionary<(int, int), (int ServiceCount, decimal TotalCost, decimal LaborCost, decimal PartsCost)>();
        foreach (var item in data)
        {
            var year = (int)item.GetType().GetProperty("Year")!.GetValue(item)!;
            var month = (int)item.GetType().GetProperty("Month")!.GetValue(item)!;
            var serviceCount = (int)item.GetType().GetProperty("ServiceCount")!.GetValue(item)!;
            var totalCost = (decimal)item.GetType().GetProperty("TotalCost")!.GetValue(item)!;
            var laborCost = (decimal)item.GetType().GetProperty("LaborCost")!.GetValue(item)!;
            var partsCost = (decimal)item.GetType().GetProperty("PartsCost")!.GetValue(item)!;
            dataDict[(year, month)] = (serviceCount, totalCost, laborCost, partsCost);
        }

        // Generate all periods and fill in gaps with zeros
        var trends = new List<MaintenanceTrendDto>();
        var currentDate = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        while (currentDate <= end)
        {
            var (serviceCount, totalCost, laborCost, partsCost) =
                dataDict.GetValueOrDefault((currentDate.Year, currentDate.Month), (0, 0, 0, 0));

            trends.Add(new MaintenanceTrendDto
            {
                Period = currentDate.ToString("MMM yyyy"),
                ServiceCount = serviceCount,
                TotalCost = totalCost,
                LaborCost = laborCost,
                PartsCost = partsCost
            });

            currentDate = currentDate.AddMonths(1);
        }

        return trends;
    }
}
