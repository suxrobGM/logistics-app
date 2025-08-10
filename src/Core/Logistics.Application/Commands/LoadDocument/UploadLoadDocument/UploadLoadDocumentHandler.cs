using Logistics.Domain.Constants;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UploadLoadDocumentHandler : RequestHandler<UploadLoadDocumentCommand, Result<Guid>>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IBlobStorageService _blobStorageService;

    public UploadLoadDocumentHandler(
        ITenantUnityOfWork tenantUow,
        IBlobStorageService blobStorageService)
    {
        _tenantUow = tenantUow;
        _blobStorageService = blobStorageService;
    }

    protected override async Task<Result<Guid>> HandleValidated(
        UploadLoadDocumentCommand req, CancellationToken cancellationToken)
    {
        // Verify load exists
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);
        if (load is null)
        {
            return Result<Guid>.Fail($"Could not find load with ID '{req.LoadId}'");
        }

        // Verify uploader exists
        var uploader = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UploadedById);
        if (uploader is null)
        {
            return Result<Guid>.Fail($"Could not find employee with ID '{req.UploadedById}'");
        }

        try
        {
            // Generate unique blob name
            var fileExtension = Path.GetExtension(req.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var blobPath = $"loads/{req.LoadId}/documents/{uniqueFileName}";

            // Upload to blob storage
            var blobUri = await _blobStorageService.UploadAsync(
                BlobConstants.DocumentsContainerName,
                blobPath,
                req.FileContent,
                req.ContentType,
                cancellationToken);

            // Create document entity
            var document = LoadDocument.Create(
                uniqueFileName,
                req.FileName,
                req.ContentType,
                req.FileSizeBytes,
                blobPath,
                BlobConstants.DocumentsContainerName,
                req.Type,
                req.LoadId,
                req.UploadedById,
                req.Description);

            // Save to database
            await _tenantUow.Repository<LoadDocument>().AddAsync(document);
            var changes = await _tenantUow.SaveChangesAsync();

            if (changes > 0)
            {
                return Result<Guid>.Succeed(document.Id);
            }

            // If database save failed, clean up blob
            await _blobStorageService.DeleteAsync(BlobConstants.DocumentsContainerName, blobPath, cancellationToken);
            return Result<Guid>.Fail("Failed to save document information to database");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Failed to upload document: {ex.Message}");
        }
    }
}
