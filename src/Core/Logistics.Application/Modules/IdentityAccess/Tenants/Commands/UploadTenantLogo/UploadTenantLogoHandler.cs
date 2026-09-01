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
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public async Task<Result<string>> Handle(UploadTenantLogoCommand req, CancellationToken ct)
    {
        // Validate file is an image
        if (!req.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Fail("File must be an image");
        }

        // Restrict to raster image extensions/types (finding #15). SVG is excluded: it can carry
        // script, and the logo is served from a public container, so an .svg/.html logo is a
        // stored-XSS vector. Bounds both the stored extension and the content type.
        var extension = Path.GetExtension(req.FileName)?.ToLowerInvariant();
        string[] allowedExtensions = [".png", ".jpg", ".jpeg", ".webp", ".gif"];
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            return Result<string>.Fail("Logo must be a PNG, JPG, WEBP, or GIF image.");
        }

        if (req.ContentType.Contains("svg", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Fail("SVG images are not allowed for logos.");
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

            // Generate unique blob path (extension validated above)
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
