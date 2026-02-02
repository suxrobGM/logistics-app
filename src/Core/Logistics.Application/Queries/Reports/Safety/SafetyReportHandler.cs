using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class SafetyReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<SafetyReportQuery, Result<SafetyReportDto>>
{
    public async Task<Result<SafetyReportDto>> Handle(SafetyReportQuery req, CancellationToken ct)
    {
        var startDate = req.StartDate == default ? DateTime.UtcNow.AddMonths(-6) : req.StartDate;
        var endDate = req.EndDate == default ? DateTime.UtcNow : req.EndDate;

        var dvirRepo = tenantUow.Repository<DvirReport>();
        var accidentRepo = tenantUow.Repository<AccidentReport>();
        var behaviorRepo = tenantUow.Repository<DriverBehaviorEvent>();

        // Build base queries with date filters
        var dvirQuery = dvirRepo.Query()
            .Where(d => d.InspectionDate >= startDate && d.InspectionDate <= endDate);
        var accidentQuery = accidentRepo.Query()
            .Where(a => a.AccidentDateTime >= startDate && a.AccidentDateTime <= endDate);
        var behaviorQuery = behaviorRepo.Query()
            .Where(b => b.OccurredAt >= startDate && b.OccurredAt <= endDate);

        // DVIR: Combined summary stats (1 query instead of 3) + status breakdown with defect count
        var dvirStatusBreakdown = await dvirQuery
            .GroupBy(d => d.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                WithDefects = g.Count(d => d.HasDefects)
            })
            .ToListAsync(ct);

        var totalDvirs = dvirStatusBreakdown.Sum(x => x.Count);
        var pendingDvirReviews = dvirStatusBreakdown
            .Where(x => x.Status == DvirStatus.Submitted)
            .Sum(x => x.Count);
        var dvirsWithDefects = dvirStatusBreakdown.Sum(x => x.WithDefects);

        // Accidents: Combined summary stats (1 query instead of 4) + status/severity breakdowns
        var accidentSummary = await accidentQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Unresolved = g.Count(a => a.Status != AccidentReportStatus.Resolved),
                EstimatedDamageCost = g.Sum(a => a.EstimatedDamageCost ?? 0),
                InjuriesReported = g.Sum(a => a.NumberOfInjuries ?? 0)
            })
            .FirstOrDefaultAsync(ct);

        var totalAccidents = accidentSummary?.Total ?? 0;
        var unresolvedAccidents = accidentSummary?.Unresolved ?? 0;
        var estimatedDamageCost = accidentSummary?.EstimatedDamageCost ?? 0;
        var injuriesReported = accidentSummary?.InjuriesReported ?? 0;

        var accidentStatusBreakdown = await accidentQuery
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var accidentSeverityBreakdown = await accidentQuery
            .GroupBy(a => a.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count(), TotalDamage = g.Sum(a => a.EstimatedDamageCost ?? 0) })
            .ToListAsync(ct);

        // Behavior: Combined summary in breakdown (1 query instead of 3)
        var behaviorEventBreakdown = await behaviorQuery
            .GroupBy(e => e.EventType)
            .Select(g => new
            {
                EventType = g.Key,
                Count = g.Count(),
                Unreviewed = g.Count(b => !b.ReviewedAt.HasValue)
            })
            .ToListAsync(ct);
        var totalBehaviorEvents = behaviorEventBreakdown.Sum(x => x.Count);
        var unreviewedBehaviorEvents = behaviorEventBreakdown.Sum(x => x.Unreviewed);

        // Trends - group by year/month
        var dvirTrends = await dvirQuery
            .GroupBy(d => new { d.InspectionDate.Year, d.InspectionDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        var accidentTrends = await accidentQuery
            .GroupBy(a => new { a.AccidentDateTime.Year, a.AccidentDateTime.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count(), Value = g.Sum(a => a.EstimatedDamageCost ?? 0) })
            .ToListAsync(ct);

        var behaviorTrends = await behaviorQuery
            .GroupBy(b => new { b.OccurredAt.Year, b.OccurredAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        // Top drivers with accident counts in single query
        var topDriverBehavior = await behaviorQuery
            .GroupBy(b => b.EmployeeId)
            .Select(g => new { DriverId = g.Key, BehaviorCount = g.Count() })
            .OrderByDescending(x => x.BehaviorCount)
            .Take(10)
            .ToListAsync(ct);

        var driverAccidentCounts = (await accidentQuery
            .GroupBy(a => a.DriverId)
            .Select(g => new { DriverId = g.Key, AccidentCount = g.Count() })
            .ToListAsync(ct))
            .ToDictionary(x => x.DriverId, x => x.AccidentCount);

        // Top trucks with defects
        var topTruckDefects = await dvirQuery
            .Where(d => d.HasDefects)
            .GroupBy(d => d.TruckId)
            .Select(g => new { TruckId = g.Key, DefectCount = g.Sum(d => d.Defects.Count) })
            .OrderByDescending(x => x.DefectCount)
            .Take(10)
            .ToListAsync(ct);

        var truckAccidentCounts = (await accidentQuery
            .GroupBy(a => a.TruckId)
            .Select(g => new { TruckId = g.Key, AccidentCount = g.Count() })
            .ToListAsync(ct))
            .ToDictionary(x => x.TruckId, x => x.AccidentCount);

        // Fetch driver names for top drivers
        var topDriverIds = topDriverBehavior.Select(x => x.DriverId).ToList();
        var driverNames = await tenantUow.Repository<Employee>().Query()
            .Where(e => topDriverIds.Contains(e.Id))
            .Select(e => new { e.Id, e.FirstName, e.LastName })
            .ToDictionaryAsync(e => e.Id, e => $"{e.FirstName} {e.LastName}", ct);

        // Fetch truck numbers for top trucks
        var topTruckIds = topTruckDefects.Select(x => x.TruckId).ToList();
        var truckNumbers = await tenantUow.Repository<Truck>().Query()
            .Where(t => topTruckIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Number })
            .ToDictionaryAsync(t => t.Id, t => t.Number, ct);

        var dto = new SafetyReportDto
        {
            // DVIR Summary
            TotalDvirs = totalDvirs,
            PendingDvirReviews = pendingDvirReviews,
            DvirsWithDefects = dvirsWithDefects,
            DvirDefectRate = totalDvirs > 0
                ? Math.Round((double)dvirsWithDefects / totalDvirs * 100, 1)
                : 0,

            // Accident Summary
            TotalAccidents = totalAccidents,
            UnresolvedAccidents = unresolvedAccidents,
            EstimatedDamageCost = estimatedDamageCost,
            InjuriesReported = injuriesReported,

            // Driver Behavior Summary
            TotalBehaviorEvents = totalBehaviorEvents,
            UnreviewedBehaviorEvents = unreviewedBehaviorEvents,

            // Breakdowns - map to DTOs with display names
            DvirStatusBreakdown = dvirStatusBreakdown
                .Select(x => new SafetyStatusBreakdownDto
                {
                    Status = x.Status.ToString(),
                    StatusDisplay = x.Status.GetDescription(),
                    Count = x.Count
                })
                .OrderByDescending(x => x.Count)
                .ToList(),

            AccidentStatusBreakdown = accidentStatusBreakdown
                .Select(x => new SafetyStatusBreakdownDto
                {
                    Status = x.Status.ToString(),
                    StatusDisplay = x.Status.GetDescription(),
                    Count = x.Count
                })
                .OrderByDescending(x => x.Count)
                .ToList(),

            AccidentSeverityBreakdown = accidentSeverityBreakdown
                .Select(x => new SafetySeverityBreakdownDto
                {
                    Severity = x.Severity.ToString(),
                    SeverityDisplay = x.Severity.GetDescription(),
                    Count = x.Count,
                    TotalDamage = x.TotalDamage
                })
                .OrderByDescending(x => x.Count)
                .ToList(),

            BehaviorEventBreakdown = behaviorEventBreakdown
                .Select(x => new SafetyEventTypeBreakdownDto
                {
                    EventType = x.EventType.ToString(),
                    EventTypeDisplay = x.EventType.GetDescription(),
                    Count = x.Count
                })
                .OrderByDescending(x => x.Count)
                .ToList(),

            // Trends - fill in gaps for months with no data
            DvirTrends = BuildTrends(dvirTrends, startDate, endDate),
            AccidentTrends = BuildAccidentTrends(accidentTrends, startDate, endDate),
            BehaviorTrends = BuildTrends(behaviorTrends, startDate, endDate),

            // Top offenders
            TopDriversByEvents = topDriverBehavior
                .Select(x => new SafetyDriverSummaryDto
                {
                    DriverId = x.DriverId,
                    DriverName = driverNames.GetValueOrDefault(x.DriverId, "Unknown"),
                    BehaviorEventCount = x.BehaviorCount,
                    AccidentCount = driverAccidentCounts.GetValueOrDefault(x.DriverId, 0)
                })
                .OrderByDescending(d => d.BehaviorEventCount + d.AccidentCount)
                .Take(5)
                .ToList(),

            TopTrucksByDefects = topTruckDefects
                .Select(x => new SafetyTruckSummaryDto
                {
                    TruckId = x.TruckId,
                    TruckNumber = truckNumbers.GetValueOrDefault(x.TruckId, "Unknown"),
                    DefectCount = x.DefectCount,
                    AccidentCount = truckAccidentCounts.GetValueOrDefault(x.TruckId, 0)
                })
                .OrderByDescending(t => t.DefectCount + t.AccidentCount)
                .Take(5)
                .ToList()
        };

        return Result<SafetyReportDto>.Ok(dto);
    }

    private static List<SafetyTrendDto> BuildTrends<T>(
        List<T> data,
        DateTime startDate,
        DateTime endDate) where T : class
    {
        // Handle anonymous types from EF Core queries
        var dataDict = new Dictionary<(int, int), int>();
        foreach (var item in data)
        {
            var year = (int)item.GetType().GetProperty("Year")!.GetValue(item)!;
            var month = (int)item.GetType().GetProperty("Month")!.GetValue(item)!;
            var count = (int)item.GetType().GetProperty("Count")!.GetValue(item)!;
            dataDict[(year, month)] = count;
        }

        return BuildTrendPeriods(startDate, endDate, (year, month) =>
            new SafetyTrendDto
            {
                Period = new DateTime(year, month, 1).ToString("MMM yyyy"),
                Count = dataDict.GetValueOrDefault((year, month), 0)
            });
    }

    private static List<SafetyTrendDto> BuildAccidentTrends<T>(
        List<T> data,
        DateTime startDate,
        DateTime endDate) where T : class
    {
        var dataDict = new Dictionary<(int, int), (int Count, decimal Value)>();
        foreach (var item in data)
        {
            var year = (int)item.GetType().GetProperty("Year")!.GetValue(item)!;
            var month = (int)item.GetType().GetProperty("Month")!.GetValue(item)!;
            var count = (int)item.GetType().GetProperty("Count")!.GetValue(item)!;
            var value = (decimal)item.GetType().GetProperty("Value")!.GetValue(item)!;
            dataDict[(year, month)] = (count, value);
        }

        return BuildTrendPeriods(startDate, endDate, (year, month) =>
        {
            var (count, value) = dataDict.GetValueOrDefault((year, month), (0, 0));
            return new SafetyTrendDto
            {
                Period = new DateTime(year, month, 1).ToString("MMM yyyy"),
                Count = count,
                Value = value
            };
        });
    }

    private static List<SafetyTrendDto> BuildTrendPeriods(
        DateTime startDate,
        DateTime endDate,
        Func<int, int, SafetyTrendDto> createTrend)
    {
        var trends = new List<SafetyTrendDto>();
        var currentDate = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        while (currentDate <= end)
        {
            trends.Add(createTrend(currentDate.Year, currentDate.Month));
            currentDate = currentDate.AddMonths(1);
        }

        return trends;
    }
}
