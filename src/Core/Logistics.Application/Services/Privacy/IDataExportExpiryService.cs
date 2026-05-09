namespace Logistics.Application.Services.Privacy;

/// <summary>
/// Daily sweeper — deletes export blobs whose retention window has elapsed
/// and marks the corresponding requests Expired.
/// </summary>
public interface IDataExportExpiryService
{
    Task ExpireAsync(CancellationToken ct = default);
}
