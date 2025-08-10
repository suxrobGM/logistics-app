using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetLoadDocumentsHandler : RequestHandler<GetLoadDocumentsQuery, Result<IEnumerable<LoadDocumentDto>>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetLoadDocumentsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<IEnumerable<LoadDocumentDto>>> HandleValidated(
        GetLoadDocumentsQuery req, CancellationToken cancellationToken)
    {
        // Verify load exists
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);
        if (load is null)
        {
            return Result<IEnumerable<LoadDocumentDto>>.Fail($"Could not find load with ID '{req.LoadId}'");
        }

        try
        {
            var query = _tenantUow.Repository<LoadDocument>()
                .Query()
                .Where(d => d.LoadId == req.LoadId);

            // Filter by status
            if (!req.IncludeDeleted)
            {
                query = query.Where(d => d.Status != DocumentStatus.Deleted);
            }

            if (req.Status.HasValue)
            {
                query = query.Where(d => d.Status == req.Status.Value);
            }

            // Filter by type
            if (req.Type.HasValue)
            {
                query = query.Where(d => d.Type == req.Type.Value);
            }

            // Include related entities
            query = query
                .Include(d => d.Load)
                .Include(d => d.UploadedBy);

            var documents = await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);

            var documentDtos = documents.Select(d => new LoadDocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                OriginalFileName = d.OriginalFileName,
                ContentType = d.ContentType,
                FileSizeBytes = d.FileSizeBytes,
                Type = d.Type,
                Status = d.Status,
                Description = d.Description,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                LoadId = d.LoadId,
                LoadName = d.Load.Name,
                LoadNumber = d.Load.Number,
                UploadedById = d.UploadedById,
                UploadedByName = $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}".Trim(),
                UploadedByEmail = d.UploadedBy.Email
            });

            return Result<IEnumerable<LoadDocumentDto>>.Succeed(documentDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<LoadDocumentDto>>.Fail($"Failed to retrieve documents: {ex.Message}");
        }
    }
}