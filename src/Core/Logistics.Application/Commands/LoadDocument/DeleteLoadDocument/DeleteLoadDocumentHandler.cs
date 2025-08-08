using Logistics.Application.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteLoadDocumentHandler : RequestHandler<DeleteLoadDocumentCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IBlobStorageService _blobStorageService;

    public DeleteLoadDocumentHandler(
        ITenantUnityOfWork tenantUow,
        IBlobStorageService blobStorageService)
    {
        _tenantUow = tenantUow;
        _blobStorageService = blobStorageService;
    }

    protected override async Task<Result> HandleValidated(
        DeleteLoadDocumentCommand req, CancellationToken cancellationToken)
    {
        // Get the document
        var document = await _tenantUow.Repository<LoadDocument>().GetByIdAsync(req.DocumentId);
        if (document is null)
        {
            return Result.Fail($"Could not find document with ID '{req.DocumentId}'");
        }

        // Check if document is already deleted
        if (document.Status == DocumentStatus.Deleted)
        {
            return Result.Fail("Document is already deleted");
        }

        // Verify requester exists
        var requester = await _tenantUow.Repository<Employee>().GetByIdAsync(req.RequestedById);
        if (requester is null)
        {
            return Result.Fail($"Could not find employee with ID '{req.RequestedById}'");
        }

        try
        {
            // Soft delete the document (mark as deleted)
            document.UpdateStatus(DocumentStatus.Deleted);

            // Save changes
            var changes = await _tenantUow.SaveChangesAsync();

            if (changes > 0)
            {
                // Delete from blob storage in background (optional - you might want to keep for audit)
                _ = Task.Run(async () =>
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

                return Result.Succeed();
            }

            return Result.Fail("Failed to delete document");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete document: {ex.Message}");
        }
    }
}