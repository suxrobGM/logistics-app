namespace Logistics.Application.Modules.Integrations.Documents.Services;

public interface IDocumentBlobCleanup : IApplicationService
{
    /// <summary>
    ///     Deletes blobs uploaded for a save that then failed, so they don't linger with no document pointing at
    ///     them. Logs and moves on when a delete fails.
    /// </summary>
    Task DeleteAsync(IEnumerable<string> blobPaths);
}
