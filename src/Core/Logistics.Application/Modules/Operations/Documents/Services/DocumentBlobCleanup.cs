using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Common.Storage;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Operations.Documents.Services;

internal sealed class DocumentBlobCleanup(IBlobStorageService blobStorage, ILogger<DocumentBlobCleanup> logger)
    : IDocumentBlobCleanup
{
    public async Task DeleteAsync(IEnumerable<string> blobPaths)
    {
        foreach (var blobPath in blobPaths.Distinct())
        {
            try
            {
                await blobStorage.DeleteAsync(BlobConstants.DocumentsContainerName, blobPath, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to clean up document blob {BlobPath}", blobPath);
            }
        }
    }
}
