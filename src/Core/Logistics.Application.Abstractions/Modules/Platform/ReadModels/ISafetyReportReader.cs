using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Application.Abstractions.Modules.Platform.ReadModels;

/// <summary>
/// Read-side projection for the Safety report. Bypasses the generic repository
/// to issue <see cref="Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking{T}"/>
/// SQL projections; composition into <c>SafetyReportDto</c> stays in the handler.
/// </summary>
public interface ISafetyReportReader
{
    /// <summary>
    /// Fetches raw safety facts (groupings, top-K lists, lookups) for the inclusive date window.
    /// Triggers tenant-database switching before the first query runs.
    /// </summary>
    Task<SafetyFacts> GetSafetyFactsAsync(DateTime startDate, DateTime endDate, CancellationToken ct);
}

/// <summary>
/// Materialized facts for one Safety report request. Each list is the result of a single SQL
/// projection; rollups (totals, breakdowns, trends, merged top-K) are computed by the handler.
/// </summary>
/// <param name="DvirByStatusAndMonth">DVIR counts grouped by status × year × month × has-defects flag.</param>
/// <param name="TopTruckDefects">Top 10 trucks by total defect count over the window.</param>
/// <param name="AccidentByStatusSeverityAndMonth">Accident counts plus summed damage and injuries grouped by status × severity × year × month.</param>
/// <param name="DriverAccidentCounts">Accident counts per driver, used to merge with <see cref="TopBehaviorByDriver"/>.</param>
/// <param name="TruckAccidentCounts">Accident counts per truck, used to merge with <see cref="TopTruckDefects"/>.</param>
/// <param name="BehaviorByTypeAndMonth">Driver-behavior event counts and unreviewed counts grouped by type × year × month.</param>
/// <param name="TopBehaviorByDriver">Top 10 drivers by total behavior-event count over the window.</param>
/// <param name="DriverNames">Display names keyed by employee id; populated only for ids in <see cref="TopBehaviorByDriver"/>.</param>
/// <param name="TruckNumbers">Truck numbers keyed by truck id; populated only for ids in <see cref="TopTruckDefects"/>.</param>
public sealed record SafetyFacts(
    IReadOnlyList<DvirGroupRow> DvirByStatusAndMonth,
    IReadOnlyList<TruckDefectRow> TopTruckDefects,
    IReadOnlyList<AccidentGroupRow> AccidentByStatusSeverityAndMonth,
    IReadOnlyList<DriverEventCountRow> DriverAccidentCounts,
    IReadOnlyList<TruckEventCountRow> TruckAccidentCounts,
    IReadOnlyList<BehaviorGroupRow> BehaviorByTypeAndMonth,
    IReadOnlyList<DriverEventCountRow> TopBehaviorByDriver,
    IReadOnlyDictionary<Guid, string> DriverNames,
    IReadOnlyDictionary<Guid, string> TruckNumbers);

/// <summary>
/// One row per (status, year, month, has-defects) DVIR group. The four-key grouping lets the handler
/// derive status breakdown, monthly trend, and the with-defects total from a single query result.
/// </summary>
public sealed record DvirGroupRow(DvirStatus Status, int Year, int Month, bool HasDefects, int Count);

/// <summary>Per-truck defect total over the date window — total count of <c>DvirDefect</c> rows whose parent DVIR is flagged with defects.</summary>
public sealed record TruckDefectRow(Guid TruckId, int DefectCount);

/// <summary>
/// One row per (status, severity, year, month) accident group. Sums damage and injuries per group
/// so the handler can roll them up without additional queries.
/// </summary>
public sealed record AccidentGroupRow(
    AccidentReportStatus Status,
    AccidentSeverity Severity,
    int Year,
    int Month,
    int Count,
    decimal Damage,
    int Injuries);

/// <summary>Event count per driver (employee id). Used for both top-K behavior lists and accident-count lookups.</summary>
public sealed record DriverEventCountRow(Guid DriverId, int Count);

/// <summary>Event count per truck. Used for accident-count lookups against the top-defect trucks.</summary>
public sealed record TruckEventCountRow(Guid TruckId, int Count);

/// <summary>One row per (event type, year, month) behavior group, including the unreviewed subset count.</summary>
public sealed record BehaviorGroupRow(DriverBehaviorEventType EventType, int Year, int Month, int Count, int Unreviewed);
