namespace Logistics.Application.Abstractions.Privacy;

/// <summary>
/// Generates a GDPR Article 15 / 20 data export ZIP for a single user, covering
/// every tenant the user has access to. Implementations stream the result so the
/// caller can upload the ZIP to blob storage without buffering the whole archive
/// in memory.
/// </summary>
public interface IDataExportService
{
    /// <summary>
    /// Build a ZIP archive containing every category of personal data we hold for
    /// <paramref name="userId"/>. The returned stream is positioned at zero and is
    /// the caller's responsibility to dispose.
    /// </summary>
    Task<Stream> GenerateExportAsync(Guid userId, CancellationToken ct = default);
}
