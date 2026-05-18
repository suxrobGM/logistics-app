using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Modules.Platform.ReadModels;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Reports.Queries;

internal sealed class SafetyReportHandler(ISafetyReportReader reader)
    : IAppRequestHandler<SafetyReportQuery, Result<SafetyReportDto>>
{
    public async Task<Result<SafetyReportDto>> Handle(SafetyReportQuery req, CancellationToken ct)
    {
        var startDate = req.StartDate == default ? DateTime.UtcNow.AddMonths(-6) : req.StartDate;
        var endDate = req.EndDate == default ? DateTime.UtcNow : req.EndDate;

        var facts = await reader.GetSafetyFactsAsync(startDate, endDate, ct);

        // DVIR rollups
        var totalDvirs = facts.DvirByStatusAndMonth.Sum(r => r.Count);
        var pendingDvirReviews = facts.DvirByStatusAndMonth
            .Where(r => r.Status == DvirStatus.Submitted)
            .Sum(r => r.Count);
        var dvirsWithDefects = facts.DvirByStatusAndMonth
            .Where(r => r.HasDefects)
            .Sum(r => r.Count);

        // Accident rollups
        var totalAccidents = facts.AccidentByStatusSeverityAndMonth.Sum(r => r.Count);
        var unresolvedAccidents = facts.AccidentByStatusSeverityAndMonth
            .Where(r => r.Status != AccidentReportStatus.Resolved)
            .Sum(r => r.Count);
        var estimatedDamageCost = facts.AccidentByStatusSeverityAndMonth.Sum(r => r.Damage);
        var injuriesReported = facts.AccidentByStatusSeverityAndMonth.Sum(r => r.Injuries);

        // Behavior rollups
        var totalBehaviorEvents = facts.BehaviorByTypeAndMonth.Sum(r => r.Count);
        var unreviewedBehaviorEvents = facts.BehaviorByTypeAndMonth.Sum(r => r.Unreviewed);

        var driverAccidentLookup = facts.DriverAccidentCounts.ToDictionary(r => r.DriverId, r => r.Count);
        var truckAccidentLookup = facts.TruckAccidentCounts.ToDictionary(r => r.TruckId, r => r.Count);

        var dto = new SafetyReportDto
        {
            // DVIR summary
            TotalDvirs = totalDvirs,
            PendingDvirReviews = pendingDvirReviews,
            DvirsWithDefects = dvirsWithDefects,
            DvirDefectRate = totalDvirs > 0
                ? Math.Round((double)dvirsWithDefects / totalDvirs * 100, 1)
                : 0,

            // Accident summary
            TotalAccidents = totalAccidents,
            UnresolvedAccidents = unresolvedAccidents,
            EstimatedDamageCost = estimatedDamageCost,
            InjuriesReported = injuriesReported,

            // Behavior summary
            TotalBehaviorEvents = totalBehaviorEvents,
            UnreviewedBehaviorEvents = unreviewedBehaviorEvents,

            // Breakdowns
            DvirStatusBreakdown = facts.DvirByStatusAndMonth
                .GroupBy(r => r.Status)
                .Select(g => new SafetyStatusBreakdownDto
                {
                    Status = g.Key.ToString(),
                    StatusDisplay = g.Key.GetDescription(),
                    Count = g.Sum(r => r.Count)
                })
                .OrderByDescending(x => x.Count)
                .ToList(),

            AccidentStatusBreakdown = facts.AccidentByStatusSeverityAndMonth
                .GroupBy(r => r.Status)
                .Select(g => new SafetyStatusBreakdownDto
                {
                    Status = g.Key.ToString(),
                    StatusDisplay = g.Key.GetDescription(),
                    Count = g.Sum(r => r.Count)
                })
                .OrderByDescending(x => x.Count)
                .ToList(),

            AccidentSeverityBreakdown = facts.AccidentByStatusSeverityAndMonth
                .GroupBy(r => r.Severity)
                .Select(g => new SafetySeverityBreakdownDto
                {
                    Severity = g.Key.ToString(),
                    SeverityDisplay = g.Key.GetDescription(),
                    Count = g.Sum(r => r.Count),
                    TotalDamage = g.Sum(r => r.Damage)
                })
                .OrderByDescending(x => x.Count)
                .ToList(),

            BehaviorEventBreakdown = facts.BehaviorByTypeAndMonth
                .GroupBy(r => r.EventType)
                .Select(g => new SafetyEventTypeBreakdownDto
                {
                    EventType = g.Key.ToString(),
                    EventTypeDisplay = g.Key.GetDescription(),
                    Count = g.Sum(r => r.Count)
                })
                .OrderByDescending(x => x.Count)
                .ToList(),

            // Trends (gap-filled)
            DvirTrends = BuildCountTrends(
                facts.DvirByStatusAndMonth.Select(r => (r.Year, r.Month, r.Count)),
                startDate, endDate),
            AccidentTrends = BuildValueTrends(
                facts.AccidentByStatusSeverityAndMonth.Select(r => (r.Year, r.Month, r.Count, r.Damage)),
                startDate, endDate),
            BehaviorTrends = BuildCountTrends(
                facts.BehaviorByTypeAndMonth.Select(r => (r.Year, r.Month, r.Count)),
                startDate, endDate),

            // Top offenders — merged behavior + accident counts per driver, defects + accidents per truck
            TopDriversByEvents = facts.TopBehaviorByDriver
                .Select(r => new SafetyDriverSummaryDto
                {
                    DriverId = r.DriverId,
                    DriverName = facts.DriverNames.GetValueOrDefault(r.DriverId, "Unknown"),
                    BehaviorEventCount = r.Count,
                    AccidentCount = driverAccidentLookup.GetValueOrDefault(r.DriverId, 0)
                })
                .OrderByDescending(d => d.BehaviorEventCount + d.AccidentCount)
                .Take(5)
                .ToList(),

            TopTrucksByDefects = facts.TopTruckDefects
                .Select(r => new SafetyTruckSummaryDto
                {
                    TruckId = r.TruckId,
                    TruckNumber = facts.TruckNumbers.GetValueOrDefault(r.TruckId, "Unknown"),
                    DefectCount = r.DefectCount,
                    AccidentCount = truckAccidentLookup.GetValueOrDefault(r.TruckId, 0)
                })
                .OrderByDescending(t => t.DefectCount + t.AccidentCount)
                .Take(5)
                .ToList()
        };

        return Result<SafetyReportDto>.Ok(dto);
    }

    private static List<SafetyTrendDto> BuildCountTrends(
        IEnumerable<(int Year, int Month, int Count)> rows,
        DateTime startDate,
        DateTime endDate)
    {
        var bucket = rows
            .GroupBy(r => (r.Year, r.Month))
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Count));

        return BuildTrendPeriods(startDate, endDate, (year, month) => new SafetyTrendDto
        {
            Period = new DateTime(year, month, 1).ToString("MMM yyyy"),
            Count = bucket.GetValueOrDefault((year, month), 0)
        });
    }

    private static List<SafetyTrendDto> BuildValueTrends(
        IEnumerable<(int Year, int Month, int Count, decimal Value)> rows,
        DateTime startDate,
        DateTime endDate)
    {
        var bucket = rows
            .GroupBy(r => (r.Year, r.Month))
            .ToDictionary(
                g => g.Key,
                g => (Count: g.Sum(r => r.Count), Value: g.Sum(r => r.Value)));

        return BuildTrendPeriods(startDate, endDate, (year, month) =>
        {
            var (count, value) = bucket.GetValueOrDefault((year, month), (0, 0m));
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
        var current = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        while (current <= end)
        {
            trends.Add(createTrend(current.Year, current.Month));
            current = current.AddMonths(1);
        }

        return trends;
    }
}
