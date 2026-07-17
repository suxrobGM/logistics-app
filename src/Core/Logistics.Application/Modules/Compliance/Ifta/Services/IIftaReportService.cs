using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

/// <summary>
/// Builds the quarterly IFTA report for the current tenant: per-jurisdiction miles from the
/// mileage rollups, gallons from fuel expenses, fleet MPG, and tax due from the published
/// master-DB rates. Closed quarters are served from their immutable snapshot; the open
/// quarter is computed live. Shared by the report query and the quarter-close job.
/// </summary>
public interface IIftaReportService : IApplicationService
{
    /// <summary>Returns the snapshot when one exists, otherwise computes live.</summary>
    Task<IftaReportDto> GetReportAsync(int year, int quarter, CancellationToken ct = default);

    /// <summary>Always computes live from rollups + expenses (used by the quarter-close job).</summary>
    Task<IftaReportDto> BuildReportAsync(int year, int quarter, CancellationToken ct = default);
}
