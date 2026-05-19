namespace Logistics.Application.Modules.Compliance.Privacy.Services;

/// <summary>
/// Processes deletion requests whose 30-day grace period has elapsed by
/// invoking <see cref="IDataAnonymizer"/> and marking each request Processed.
/// </summary>
public interface IDataDeletionProcessingService : IApplicationService
{
    Task ProcessDueAsync(CancellationToken ct = default);
}
