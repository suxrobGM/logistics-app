using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

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
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, cancellationToken);
        if (load is null)
        {
            return Result<IEnumerable<LoadDocumentDto>>.Fail($"Could not find load with ID '{req.LoadId}'");
        }

        try
        {
            var loadDocuments = _tenantUow.Repository<LoadDocument>()
                .ApplySpecification(new FilterDocumentsByType(req.Type, req.Status));

            var documentDtos = loadDocuments.Select(d => new LoadDocumentDto
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
            }).ToList();

            return Result<IEnumerable<LoadDocumentDto>>.Succeed(documentDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<LoadDocumentDto>>.Fail($"Failed to retrieve documents: {ex.Message}");
        }
    }
}
