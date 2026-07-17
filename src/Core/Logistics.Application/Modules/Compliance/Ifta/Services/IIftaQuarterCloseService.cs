using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

/// <summary>
/// Writes the immutable snapshot for the most recently ended quarter (once — snapshots are
/// never updated) and purges location breadcrumbs past the 4-year IFTA audit window.
/// Operates on the current tenant; the Hangfire job iterates tenants.
/// </summary>
public interface IIftaQuarterCloseService : IApplicationService
{
    /// <summary>Snapshots the previous quarter if no snapshot exists yet. Returns true when one was written.</summary>
    Task<bool> CloseQuarterForCurrentTenantAsync(CancellationToken ct = default);

    /// <summary>Deletes TruckLocationHistory rows older than the 4-year audit window. Returns rows purged.</summary>
    Task<int> PurgeExpiredLocationHistoryAsync(CancellationToken ct = default);
}
