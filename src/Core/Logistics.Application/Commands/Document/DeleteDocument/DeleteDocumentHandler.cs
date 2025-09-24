using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteDocumentHandler : IAppRequestHandler<DeleteDocumentCommand, Result>
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ITenantUnitOfWork _tenantUow;

    public DeleteDocumentHandler(
        ITenantUnitOfWork tenantUow,
        IBlobStorageService blobStorageService)
    {
        _tenantUow = tenantUow;
        _blobStorageService = blobStorageService;
    }

    public async Task<Result> Handle(
        DeleteDocumentCommand req, CancellationToken ct)
    {
        // Get the document

        Document? document = await _tenantUow.Repository<LoadDocument>().GetByIdAsync(req.DocumentId, ct);

        if (document == null)
        {
            document = await _tenantUow.Repository<EmployeeDocument>().GetByIdAsync(req.DocumentId, ct);
        }

        if (document is null)
        {
            return Result.Fail($"Could not find document with ID '{req.DocumentId}'");
        }

        // Check if the document is already deleted
        if (document.Status == DocumentStatus.Deleted)
        {
            return Result.Fail("Document is already deleted");
        }

        try
        {
            // Soft delete the document (mark as deleted)
            document.UpdateStatus(DocumentStatus.Deleted);

            // Save changes
            var changes = await _tenantUow.SaveChangesAsync();

            if (changes > 0)
            {
                _ = DeleteDocumentBackground(document, ct);
                return Result.Ok();
            }

            return Result.Fail("Failed to delete document");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete document: {ex.Message}");
        }
    }

    private Task DeleteDocumentBackground(Document document, CancellationToken cancellationToken)
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
