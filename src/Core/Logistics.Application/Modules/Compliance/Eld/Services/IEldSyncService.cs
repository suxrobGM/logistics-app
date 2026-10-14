using Logistics.Application.Abstractions.Eld;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Compliance.Eld.Services;

/// <summary>
/// The one place ELD data is pulled into the tenant database: the recurring job, the manual sync
/// endpoint and the webhook handler all go through it. Expects the tenant context to be set and
/// saves its own changes.
/// </summary>
public interface IEldSyncService : IApplicationService
{
    /// <summary>
    /// Current HOS clocks for every mapped driver and, for GPS-capable providers, the latest position of
    /// every mapped vehicle (recorded through <c>ITruckLocationRecorder</c> so IFTA mileage accrues).
    /// Positions are broadcast to the tenant's live map.
    /// </summary>
    Task SyncLiveAsync(EldProviderConfiguration config, CancellationToken ct);

    /// <summary>
    /// HOS logs and violations from the last 8 days for every mapped driver. Eight days is the FMCSA
    /// 70-hour/8-day cycle, so an edit to any log that still counts is picked up.
    /// </summary>
    Task SyncHistoryAsync(EldProviderConfiguration config, CancellationToken ct);

    /// <summary>Re-reads one driver's current clocks (a duty-status webhook).</summary>
    Task SyncDriverHosStatusAsync(
        IEldProviderService provider, EldDriverMapping mapping, CancellationToken ct);

    /// <summary>Re-reads one driver's violations over the same window (a violation webhook).</summary>
    Task SyncDriverViolationsAsync(
        IEldProviderService provider, EldDriverMapping mapping, CancellationToken ct);
}
