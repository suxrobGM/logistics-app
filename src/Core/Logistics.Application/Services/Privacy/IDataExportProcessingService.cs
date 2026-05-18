namespace Logistics.Application.Services.Privacy;

/// <summary>
/// Picks up pending GDPR data export requests, generates the ZIP, uploads it
/// to blob storage, and notifies the user. Designed to be invoked by Hangfire.
/// </summary>
public interface IDataExportProcessingService : IApplicationService
{
    Task ProcessPendingAsync(CancellationToken ct = default);
}
