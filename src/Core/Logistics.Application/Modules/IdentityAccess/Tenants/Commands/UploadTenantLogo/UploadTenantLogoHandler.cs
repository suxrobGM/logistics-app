using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Storage;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class UploadTenantLogoHandler(
    IMasterUnitOfWork masterUow,
    IBlobStorageService blobStorageService,
    ILogger<UploadTenantLogoHandler> logger)
    : IAppRequestHandler<UploadTenantLogoCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadTenantLogoCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);
        if (tenant is null)
        {
            return Result<string>.Fail($"Could not find a tenant with ID '{req.TenantId}'");
        }

        try
        {
            // Delete existing logo if present
            if (!string.IsNullOrEmpty(tenant.LogoPath))
            {
                try
                {
                    await blobStorageService.DeleteAsync(BlobConstants.LogosContainerName, tenant.LogoPath, ct);
                }
                catch
                {
                    // Ignore deletion errors for old logo
                }
            }

            var extension = Path.GetExtension(req.FileName).ToLowerInvariant();
            var blobPath = $"tenants/{req.TenantId}/logo{extension}";

            await blobStorageService.UploadAsync(
                BlobConstants.LogosContainerName,
                blobPath,
                req.FileContent,
                req.ContentType,
                ct);

            // Update tenant's logo path
            tenant.LogoPath = blobPath;
            masterUow.Repository<Tenant>().Update(tenant);
            await masterUow.SaveChangesAsync(ct);

            return Result<string>.Ok(blobPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload logo for tenant {TenantId}", req.TenantId);
            return Result<string>.Fail("Failed to upload logo.");
        }
    }
}
