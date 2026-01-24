using Logistics.Application.Abstractions;
using Logistics.Application.Constants;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UploadTenantLogoHandler(
    IMasterUnitOfWork masterUow,
    IBlobStorageService blobStorageService)
    : IAppRequestHandler<UploadTenantLogoCommand, Result<string>>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public async Task<Result<string>> Handle(UploadTenantLogoCommand req, CancellationToken ct)
    {
        // Validate file is an image
        if (!req.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Fail("File must be an image");
        }

        // Validate file size
        if (req.FileSizeBytes > MaxFileSizeBytes)
        {
            return Result<string>.Fail("File size exceeds the maximum allowed (5 MB)");
        }

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

            // Generate unique blob path
            var extension = Path.GetExtension(req.FileName);
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
            return Result<string>.Fail($"Failed to upload logo: {ex.Message}");
        }
    }
}
