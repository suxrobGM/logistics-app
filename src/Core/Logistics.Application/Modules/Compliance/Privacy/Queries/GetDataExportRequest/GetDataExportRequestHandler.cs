using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Application.Modules.Compliance.Privacy.Queries;

internal sealed class GetDataExportRequestHandler(
    IMasterUnitOfWork masterUow,
    ICurrentUserService currentUserService,
    IBlobStorageService blobStorage)
    : IAppRequestHandler<GetDataExportRequestQuery, Result<DataExportRequestDto>>
{
    public async Task<Result<DataExportRequestDto>> Handle(GetDataExportRequestQuery req, CancellationToken ct)
    {
        var userId = currentUserService.GetUserId();
        if (userId is null)
        {
            return Result<DataExportRequestDto>.Fail("User not authenticated.");
        }

        var request = await masterUow.Repository<DataExportRequest>().GetByIdAsync(req.Id, ct);
        if (request is null || request.UserId != userId.Value)
        {
            return Result<DataExportRequestDto>.Fail("Data export request not found.");
        }

        var dto = request.ToDto();

        if (request.Status == DataExportStatus.Ready
            && !string.IsNullOrEmpty(request.BlobContainer)
            && !string.IsNullOrEmpty(request.BlobName)
            && request.BlobTenantId is not null
            && request.ExpiresAt > DateTime.UtcNow)
        {
            // Use the tenant context that the upload job recorded â€” not the caller's
            // current tenant â€” so a multi-tenant user always reaches the same blob.
            var url = await blobStorage.GetSignedUrlAsync(
                request.BlobContainer,
                request.BlobName,
                PrivacyDefaults.ExportSignedUrlLifetime,
                request.BlobTenantId.Value,
                ct);

            dto = dto with { DownloadUrl = url };
        }

        return Result<DataExportRequestDto>.Ok(dto);
    }
}
