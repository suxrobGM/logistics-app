using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Common.Constants;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

public static class DocumentBlobCleanup
{
    public static async Task DeleteAsync(
        IBlobStorageService blobStorage,
        IEnumerable<string> blobPaths,
        ILogger logger)
    {
        foreach (var blobPath in blobPaths.Distinct())
        {
            try
            {
                await blobStorage.DeleteAsync(
                    BlobConstants.DocumentsContainerName,
                    blobPath,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to clean up document blob {BlobPath}", blobPath);
            }
        }
    }
}
