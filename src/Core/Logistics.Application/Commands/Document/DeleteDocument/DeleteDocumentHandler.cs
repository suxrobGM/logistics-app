using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteDocumentHandler : RequestHandler<DeleteDocumentCommand, Result>
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ITenantUnityOfWork _tenantUow;

    public DeleteDocumentHandler(
        ITenantUnityOfWork tenantUow,
        IBlobStorageService blobStorageService)
    {
        _tenantUow = tenantUow;
        _blobStorageService = blobStorageService;
    }

    protected override async Task<Result> HandleValidated(
        DeleteDocumentCommand req, CancellationToken ct)
    {
        // Get the document
        var document = await _tenantUow.Repository<LoadDocument>().GetByIdAsync(req.DocumentId, ct);
        if (document is null) return Result.Fail($"Could not find document with ID '{req.DocumentId}'");

        // Check if the document is already deleted
        if (document.Status == DocumentStatus.Deleted) return Result.Fail("Document is already deleted");

        try
        {
            // Soft delete the document (mark as deleted)
            document.UpdateStatus(DocumentStatus.Deleted);

            // Save changes
            var changes = await _tenantUow.SaveChangesAsync();

            if (changes > 0)
            {
                _ = DeleteDocumentBackground(document, ct);
                return Result.Succeed();
            }

            return Result.Fail("Failed to delete document");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete document: {ex.Message}");
        }
    }

    private Task DeleteDocumentBackground(LoadDocument document, CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            try
            {
                await _blobStorageService.DeleteAsync(document.BlobContainer, document.BlobPath, cancellationToken);
            }
            catch
            {
                // Log error but don't fail the operation
                // The blob can be cleaned up later
            }
        }, cancellationToken);
    }
}
