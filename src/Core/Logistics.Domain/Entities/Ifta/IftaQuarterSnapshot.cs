using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// Immutable-by-convention snapshot of a closed IFTA quarter, written once by the
/// quarter-close job. Closed quarters are served verbatim from <see cref="ReportJson"/>
/// (no update command exists); only the open quarter is computed live.
/// </summary>
public class IftaQuarterSnapshot : AuditableEntity, ITenantEntity
{
    public required int Year { get; set; }

    /// <summary>Calendar quarter, 1-4.</summary>
    public required int Quarter { get; set; }

    public required DateTime ClosedAt { get; set; }

    /// <summary>
    /// Full serialized IftaReportDto for the quarter — the single source of truth. Totals are
    /// deliberately not denormalized into columns: nothing queries them, and a second copy can
    /// only drift from this one.
    /// </summary>
    public required string ReportJson { get; set; }
}
