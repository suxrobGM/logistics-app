namespace Logistics.Application.Services.Privacy;

/// <summary>
/// Processes deletion requests whose 30-day grace period has elapsed by
/// invoking <see cref="IDataAnonymizer"/> and marking each request Processed.
/// </summary>
public interface IDataDeletionProcessingService
{
    Task ProcessDueAsync(CancellationToken ct = default);
}
