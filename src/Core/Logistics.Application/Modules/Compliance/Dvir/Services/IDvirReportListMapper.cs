using Logistics.Domain.Entities.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Services;

public interface IDvirReportListMapper : IApplicationService
{
    /// <summary>Maps a list of reports with a fixed number of queries, however many rows there are.</summary>
    Task<IEnumerable<DvirReportDto>> MapAsync(IReadOnlyList<DvirReport> reports, CancellationToken ct);
}
