namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>
/// The one place tenant data is pushed to the accounting provider: the recurring job and the manual
/// "Sync now" both go through it. Expects the tenant context to be set and saves its own changes.
/// </summary>
public interface IAccountingSyncService : IApplicationService
{
    /// <summary>
    /// Push every customer, invoice, payment, expense and approved driver pay changed since its last push.
    /// Does nothing when the tenant's plan lacks accounting or the connection needs a reconnect. A record
    /// that failed is tried again on the next run.
    /// </summary>
    Task SyncAsync(CancellationToken ct);
}
