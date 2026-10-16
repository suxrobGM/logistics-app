namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>Pushes what customers owe and pay: customers, invoices and payments, retiring removed ones.</summary>
internal interface IAccountingReceivablesSyncService : IApplicationService
{
    Task PushAsync(SyncRun run, CancellationToken ct);
}
