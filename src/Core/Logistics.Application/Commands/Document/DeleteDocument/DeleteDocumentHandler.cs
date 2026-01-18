using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeleteDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorageService,
    ILogger<DeleteDocumentHandler> logger)
    : IAppRequestHandler<DeleteDocumentCommand, Result>
{
    public async Task<Result> Handle(
        DeleteDocumentCommand req, CancellationToken ct)
    {
        var documentRepo = tenantUow.Repository<Document>();
        var document = await documentRepo.GetByIdAsync(req.DocumentId, ct);

        if (document is null)
        {
            return Result.Fail($"Could not find document with ID '{req.DocumentId}'");
        }

        try
        {
            // Store blob info before deletion
            var blobContainer = document.BlobContainer;
            var blobPath = document.BlobPath;

            // Hard delete the document from database
            documentRepo.Delete(document);
            var changes = await tenantUow.SaveChangesAsync(ct);

            if (changes > 0)
            {
                // Delete blob in background
                _ = DeleteBlobAsync(blobContainer, blobPath, ct);
                return Result.Ok();
            }

            return Result.Fail("Failed to delete document");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete document: {ex.Message}");
        }
    }

    private Task DeleteBlobAsync(string blobContainer, string blobPath, CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            try
            {
                await blobStorageService.DeleteAsync(blobContainer, blobPath, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete blob {BlobPath} from container {BlobContainer}", blobPath,
                    blobContainer);
            }
        }, ct);
    }
}
