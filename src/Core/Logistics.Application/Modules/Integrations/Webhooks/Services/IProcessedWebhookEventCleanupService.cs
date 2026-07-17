namespace Logistics.Application.Modules.Integrations.Webhooks.Services;

/// <summary>
/// Daily housekeeping — prunes processed-webhook idempotency rows (master DB) once they are old
/// enough that no provider would still retry the original delivery.
/// </summary>
public interface IProcessedWebhookEventCleanupService : IApplicationService
{
    Task CleanupAsync(CancellationToken ct = default);
}
