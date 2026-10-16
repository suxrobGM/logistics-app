namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>Pushes what the company spends: factoring fees, expenses and driver pay.</summary>
internal interface IAccountingPayablesSyncService : IApplicationService
{
    Task PushAsync(SyncRun run, CancellationToken ct);
}
