using Logistics.Application.Abstractions.Modules.Platform.ReadModels;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Reads.Platform;

internal sealed class SafetyReportReader(TenantDbContext db, ITenantUnitOfWork tenantUow) : ISafetyReportReader
{
    private const int TopK = 10;

    public async Task<SafetyFacts> GetSafetyFactsAsync(DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        // Ensure tenant-specific connection is active before any query runs.
        _ = tenantUow.GetCurrentTenant();

        var dvirByStatusAndMonth = await db.Set<DvirReport>()
            .AsNoTracking()
            .Where(d => d.InspectionDate >= startDate && d.InspectionDate <= endDate)
            .GroupBy(d => new { d.Status, d.InspectionDate.Year, d.InspectionDate.Month, d.HasDefects })
            .Select(g => new DvirGroupRow(g.Key.Status, g.Key.Year, g.Key.Month, g.Key.HasDefects, g.Count()))
            .ToListAsync(ct);

        var topTruckDefects = await (
                from def in db.Set<DvirDefect>().AsNoTracking()
                join r in db.Set<DvirReport>() on def.DvirReportId equals r.Id
                where r.InspectionDate >= startDate && r.InspectionDate <= endDate && r.HasDefects
                group def by r.TruckId into g
                select new TruckDefectRow(g.Key, g.Count()))
            .OrderByDescending(x => x.DefectCount)
            .Take(TopK)
            .ToListAsync(ct);

        var accidentByStatusSeverityAndMonth = await db.Set<AccidentReport>()
            .AsNoTracking()
            .Where(a => a.AccidentDateTime >= startDate && a.AccidentDateTime <= endDate)
            .GroupBy(a => new { a.Status, a.Severity, a.AccidentDateTime.Year, a.AccidentDateTime.Month })
            .Select(g => new AccidentGroupRow(
                g.Key.Status,
                g.Key.Severity,
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Sum(a => a.EstimatedDamageCost ?? 0m),
                g.Sum(a => a.NumberOfInjuries ?? 0)))
            .ToListAsync(ct);

        var driverAccidentCounts = await db.Set<AccidentReport>()
            .AsNoTracking()
            .Where(a => a.AccidentDateTime >= startDate && a.AccidentDateTime <= endDate)
            .GroupBy(a => a.DriverId)
            .Select(g => new DriverEventCountRow(g.Key, g.Count()))
            .ToListAsync(ct);

        var truckAccidentCounts = await db.Set<AccidentReport>()
            .AsNoTracking()
            .Where(a => a.AccidentDateTime >= startDate && a.AccidentDateTime <= endDate)
            .GroupBy(a => a.TruckId)
            .Select(g => new TruckEventCountRow(g.Key, g.Count()))
            .ToListAsync(ct);

        var behaviorByTypeAndMonth = await db.Set<DriverBehaviorEvent>()
            .AsNoTracking()
            .Where(b => b.OccurredAt >= startDate && b.OccurredAt <= endDate)
            .GroupBy(b => new { b.EventType, b.OccurredAt.Year, b.OccurredAt.Month })
            .Select(g => new BehaviorGroupRow(
                g.Key.EventType,
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Count(e => !e.ReviewedAt.HasValue)))
            .ToListAsync(ct);

        var topBehaviorByDriver = await db.Set<DriverBehaviorEvent>()
            .AsNoTracking()
            .Where(b => b.OccurredAt >= startDate && b.OccurredAt <= endDate)
            .GroupBy(b => b.EmployeeId)
            .Select(g => new DriverEventCountRow(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(TopK)
            .ToListAsync(ct);

        // Only fetch lookup names for IDs actually referenced by the top-K sets.
        var topDriverIds = topBehaviorByDriver.Select(x => x.DriverId).ToList();
        var driverNames = topDriverIds.Count == 0
            ? []
            : await db.Set<Employee>()
                .AsNoTracking()
                .Where(e => topDriverIds.Contains(e.Id))
                .Select(e => new { e.Id, e.FirstName, e.LastName })
                .ToDictionaryAsync(e => e.Id, e => $"{e.FirstName} {e.LastName}", ct);

        var topTruckIds = topTruckDefects.Select(x => x.TruckId).ToList();
        var truckNumbers = topTruckIds.Count == 0
            ? []
            : await db.Set<Truck>()
                .AsNoTracking()
                .Where(t => topTruckIds.Contains(t.Id))
                .Select(t => new { t.Id, t.Number })
                .ToDictionaryAsync(t => t.Id, t => t.Number, ct);

        return new SafetyFacts(
            dvirByStatusAndMonth,
            topTruckDefects,
            accidentByStatusSeverityAndMonth,
            driverAccidentCounts,
            truckAccidentCounts,
            behaviorByTypeAndMonth,
            topBehaviorByDriver,
            driverNames,
            truckNumbers);
    }
}
