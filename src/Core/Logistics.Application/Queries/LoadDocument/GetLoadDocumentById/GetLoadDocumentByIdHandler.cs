using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetLoadDocumentByIdHandler : RequestHandler<GetLoadDocumentByIdQuery, Result<LoadDocumentDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetLoadDocumentByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<LoadDocumentDto>> HandleValidated(
        GetLoadDocumentByIdQuery req, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _tenantUow.Repository<LoadDocument>()
                .Query()
                .Include(d => d.Load)
                .Include(d => d.UploadedBy)
                .FirstOrDefaultAsync(d => d.Id == req.DocumentId, cancellationToken);

            if (document is null)
            {
                return Result<LoadDocumentDto>.Fail($"Could not find document with ID '{req.DocumentId}'");
            }

            // Check if document is deleted
            if (document.Status == DocumentStatus.Deleted)
            {
                return Result<LoadDocumentDto>.Fail("Document has been deleted");
            }

            var documentDto = new LoadDocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FileSizeBytes = document.FileSizeBytes,
                Type = document.Type,
                Status = document.Status,
                Description = document.Description,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                LoadId = document.LoadId,
                LoadName = document.Load.Name,
                LoadNumber = document.Load.Number,
                UploadedById = document.UploadedById,
                UploadedByName = $"{document.UploadedBy.FirstName} {document.UploadedBy.LastName}".Trim(),
                UploadedByEmail = document.UploadedBy.Email
            };

            return Result<LoadDocumentDto>.Succeed(documentDto);
        }
        catch (Exception ex)
        {
            return Result<LoadDocumentDto>.Fail($"Failed to retrieve document: {ex.Message}");
        }
    }
}