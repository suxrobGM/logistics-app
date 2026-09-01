using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Storage;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

internal sealed class UploadDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorageService,
    ICurrentUserService currentUserService,
    ILogger<UploadDocumentHandler> logger)
    : IAppRequestHandler<UploadDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadDocumentCommand req, CancellationToken ct)
    {
        var access = await DocumentAccess.ResolveAsync(tenantUow, currentUserService, ct);
        if (access is null ||
            !await DocumentAccess.CanAccessOwnerAsync(tenantUow, access, req.OwnerType, req.OwnerId, ct))
        {
            return Result<Guid>.Fail("Owner not found or access denied.");
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

            _ = await blobStorageService.UploadAsync(
                BlobConstants.DocumentsContainerName,
                blobPath,
                req.FileContent,
                req.ContentType,
                ct);

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
                    access.CallerId,
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
                    access.CallerId,
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
                    access.CallerId,
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

            logger.LogWarning(
                "Failed to save document to database, rolling back blob: {BlobPath}", blobPath);
            await blobStorageService.DeleteAsync(BlobConstants.DocumentsContainerName, blobPath, ct);
            return Result<Guid>.Fail("Failed to save document information to database");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload document for {OwnerType}/{OwnerId}", req.OwnerType, req.OwnerId);
            return Result<Guid>.Fail("Failed to upload document.");
        }
    }
}
