namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

/// <summary>Skips the LoadBoard feature check, because the calling job already does it.</summary>
public interface ILoadBoardSyncService : IApplicationService
{
    /// <summary>Catches posts that DAT expired or a dispatcher removed in DAT One.</summary>
    Task ReconcilePostedTrucksAsync(CancellationToken ct);

    /// <summary>Deletes rows only after 30 days expired, so recent history stays visible.</summary>
    Task CleanupExpiredDataAsync(CancellationToken ct);
}
