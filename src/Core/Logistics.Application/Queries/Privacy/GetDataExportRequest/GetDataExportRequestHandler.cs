using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDataExportRequestHandler(
    IMasterUnitOfWork masterUow,
    ICurrentUserService currentUserService,
    IBlobStorageService blobStorage,
    ITenantService tenantService)
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
            && request.ExpiresAt > DateTime.UtcNow)
        {
            var tenantId = tenantService.GetCurrentTenant().Id;
            var url = await blobStorage.GetSignedUrlAsync(
                request.BlobContainer,
                request.BlobName,
                PrivacyDefaults.ExportSignedUrlLifetime,
                tenantId,
                ct);

            dto = dto with { DownloadUrl = url };
        }

        return Result<DataExportRequestDto>.Ok(dto);
    }
}
