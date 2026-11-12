namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

/// <summary>
/// Keeps the current tenant's load board data in step with the boards. Does not check the
/// LoadBoard feature; the caller does.
/// </summary>
public interface ILoadBoardSyncService : IApplicationService
{
    /// <summary>
    /// Marks a truck post expired when its board no longer lists it (DAT expired it, or a
    /// dispatcher took it down in DAT One), and refreshes the expiry of the posts still listed.
    /// </summary>
    Task ReconcilePostedTrucksAsync(CancellationToken ct);

    /// <summary>
    /// Expires listings and truck posts past their window, then deletes the ones that have been
    /// expired for more than 30 days.
    /// </summary>
    Task CleanupExpiredDataAsync(CancellationToken ct);
}
