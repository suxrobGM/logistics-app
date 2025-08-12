using Logistics.Domain.Constants;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Services;
using Logistics.Shared.Models;

using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UploadDocumentHandler : RequestHandler<UploadDocumentCommand, Result<Guid>>
{
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<UploadDocumentHandler> _logger;
    private readonly ITenantUnityOfWork _tenantUow;

    public UploadDocumentHandler(
        ITenantUnityOfWork tenantUow,
        IBlobStorageService blobStorageService,
        ILogger<UploadDocumentHandler> logger)
    {
        _tenantUow = tenantUow;
        _blobStorage = blobStorageService;
        _logger = logger;
    }

    protected override async Task<Result<Guid>> HandleValidated(
        UploadDocumentCommand req, CancellationToken ct)
    {
        switch (req.OwnerType)
        {
            case DocumentOwnerType.Load:
                if (await _tenantUow.Repository<Load>().GetByIdAsync(req.OwnerId, ct) is null)
                    return Result<Guid>.Fail($"Could not find load with ID '{req.OwnerId}'");
                break;

            case DocumentOwnerType.Employee:
                if (await _tenantUow.Repository<Employee>().GetByIdAsync(req.OwnerId, ct) is null)
                    return Result<Guid>.Fail($"Could not find employee with ID '{req.OwnerId}'");
                break;
        }

        // Verify uploader exists
        if (await _tenantUow.Repository<Employee>().GetByIdAsync(req.UploadedById, ct) is null)
            return Result<Guid>.Fail($"Could not find employee with ID '{req.UploadedById}'");

        try
        {
            var ext = Path.GetExtension(req.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var ownerSegment = req.OwnerType == DocumentOwnerType.Load ? "loads" : "employees";
            var blobPath = $"{ownerSegment}/{req.OwnerId}/documents/{uniqueFileName}";

            // Upload to blob
            _ = await _blobStorage.UploadAsync(
                BlobConstants.DocumentsContainerName,
                blobPath,
                req.FileContent,
                req.ContentType,
                ct);

            // Create the entity (derived)
            Guid newId;
            if (req.OwnerType == DocumentOwnerType.Load)
            {
                var entity = LoadDocument.Create(
                    uniqueFileName,
                    req.FileName,
                    req.ContentType,
                    req.FileSizeBytes,
                    blobPath,
                    BlobConstants.DocumentsContainerName,
                    req.Type,
                    req.OwnerId,
                    req.UploadedById,
                    req.Description);

                await _tenantUow.Repository<LoadDocument>().AddAsync(entity, ct);
                newId = entity.Id;
            }
            else
            {
                var entity = EmployeeDocument.Create(
                    uniqueFileName,
                    req.FileName,
                    req.ContentType,
                    req.FileSizeBytes,
                    blobPath,
                    BlobConstants.DocumentsContainerName,
                    req.Type,
                    req.OwnerId,
                    req.UploadedById,
                    req.Description);

                await _tenantUow.Repository<EmployeeDocument>().AddAsync(entity, ct);
                newId = entity.Id;
            }

            var changes = await _tenantUow.SaveChangesAsync();
            if (changes > 0)
                return Result<Guid>.Succeed(newId);

            // rollback blob if DB save failed
            await _blobStorage.DeleteAsync(BlobConstants.DocumentsContainerName, blobPath, ct);
            return Result<Guid>.Fail("Failed to save document information to database");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Failed to upload document: {ex.Message}");
        }
    }
}
