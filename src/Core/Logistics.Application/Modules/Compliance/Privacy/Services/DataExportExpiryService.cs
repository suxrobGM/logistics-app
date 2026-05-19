using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Storage;

namespace Logistics.Application.Modules.Compliance.Privacy.Services;

internal sealed class DataExportExpiryService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorage,
    ILogger<DataExportExpiryService> logger) : IDataExportExpiryService
{
    public async Task ExpireAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var expired = await masterUow.Repository<DataExportRequest>()
            .Query()
            .Where(r => r.Status == DataExportStatus.Ready
                        && r.ExpiresAt != null
                        && r.ExpiresAt <= now)
            .ToListAsync(ct);

        foreach (var request in expired)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                if (!string.IsNullOrEmpty(request.BlobContainer)
                    && !string.IsNullOrEmpty(request.BlobName)
                    && request.BlobTenantId is not null)
                {
                    var anchor = await masterUow.Repository<Tenant>()
                        .GetByIdAsync(request.BlobTenantId.Value, ct);

                    if (anchor is not null)
                    {
                        tenantUow.SetCurrentTenant(anchor);
                        await blobStorage.DeleteAsync(request.BlobContainer, request.BlobName, ct);
                    }
                }

                request.Status = DataExportStatus.Expired;
                request.BlobContainer = null;
                request.BlobName = null;
                request.BlobTenantId = null;
            }
            catch (Exception ex)
            {
                // Mark expired anyway so we don't retry forever; log for ops cleanup.
                logger.LogError(ex,
                    "Failed to delete blob for expired export '{RequestId}'; marking expired.", request.Id);
                request.Status = DataExportStatus.Expired;
            }
        }

        if (expired.Count > 0)
        {
            await masterUow.SaveChangesAsync(ct);
            logger.LogInformation("Marked {Count} data exports as expired.", expired.Count);
        }
    }
}
