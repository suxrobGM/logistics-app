using Logistics.Application.Abstractions;
using Logistics.Application.Constants;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UploadDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorageService,
    ILogger<UploadDocumentHandler> logger)
    : IAppRequestHandler<UploadDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadDocumentCommand req, CancellationToken ct)
    {
        switch (req.OwnerType)
        {
            case DocumentOwnerType.Load:
                if (await tenantUow.Repository<Load>().GetByIdAsync(req.OwnerId, ct) is null)
                {
                    return Result<Guid>.Fail($"Could not find load with ID '{req.OwnerId}'");
                }

                break;
            case DocumentOwnerType.Employee:
                if (await tenantUow.Repository<Employee>().GetByIdAsync(req.OwnerId, ct) is null)
                {
                    return Result<Guid>.Fail($"Could not find employee with ID '{req.OwnerId}'");
                }

                break;
            case DocumentOwnerType.Truck:
                if (await tenantUow.Repository<Truck>().GetByIdAsync(req.OwnerId, ct) is null)
                {
                    return Result<Guid>.Fail($"Could not find truck with ID '{req.OwnerId}'");
                }

                break;
        }

        // Verify uploader exists
        if (await tenantUow.Repository<Employee>().GetByIdAsync(req.UploadedById, ct) is null)
        {
            return Result<Guid>.Fail($"Could not find employee with ID '{req.UploadedById}'");
        }

        try
        {
            var uniqueFileName = BlobPathHelper.GenerateUniqueFileName(req.FileName);
            var ownerSegment = req.OwnerType switch
            {
                DocumentOwnerType.Load => "loads",
                DocumentOwnerType.Employee => "employees",
                DocumentOwnerType.Truck => "trucks",
                _ => "other"
            };
            var blobPath = BlobPathHelper.GetOwnerDocumentBlobPath(ownerSegment, req.OwnerId, uniqueFileName);

            // Upload to blob
            _ = await blobStorageService.UploadAsync(
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

                await tenantUow.Repository<LoadDocument>().AddAsync(entity, ct);
                newId = entity.Id;
            }
            else if (req.OwnerType == DocumentOwnerType.Truck)
            {
                var entity = TruckDocument.Create(
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

                await tenantUow.Repository<TruckDocument>().AddAsync(entity, ct);
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

                await tenantUow.Repository<EmployeeDocument>().AddAsync(entity, ct);
                newId = entity.Id;
            }

            int changes = await tenantUow.SaveChangesAsync(ct);
            if (changes > 0)
            {
                logger.LogInformation(
                    "Document uploaded: {DocumentId}, Type: {DocumentType}, Owner: {OwnerType}/{OwnerId}, File: {FileName}",
                    newId, req.Type, req.OwnerType, req.OwnerId, req.FileName);
                return Result<Guid>.Ok(newId);
            }

            // rollback blob if DB save failed
            logger.LogWarning(
                "Failed to save document to database, rolling back blob: {BlobPath}", blobPath);
            await blobStorageService.DeleteAsync(BlobConstants.DocumentsContainerName, blobPath, ct);
            return Result<Guid>.Fail("Failed to save document information to database");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload document for {OwnerType}/{OwnerId}", req.OwnerType, req.OwnerId);
            return Result<Guid>.Fail($"Failed to upload document: {ex.Message}");
        }
    }
}
