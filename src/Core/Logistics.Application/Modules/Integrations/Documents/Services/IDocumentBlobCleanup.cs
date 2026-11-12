namespace Logistics.Application.Modules.Integrations.Documents.Services;

public interface IDocumentBlobCleanup : IApplicationService
{
    /// <summary>Removes blobs orphaned by a failed save; logs and skips any delete that fails.</summary>
    Task DeleteAsync(IEnumerable<string> blobPaths);
}
