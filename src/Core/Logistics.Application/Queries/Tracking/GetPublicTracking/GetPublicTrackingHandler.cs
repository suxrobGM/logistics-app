using Logistics.Application.Abstractions;
using Logistics.Application.Constants;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPublicTrackingHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorageService)
    : IAppRequestHandler<GetPublicTrackingQuery, Result<PublicTrackingDto>>
{
    public async Task<Result<PublicTrackingDto>> Handle(
        GetPublicTrackingQuery req,
        CancellationToken ct)
    {
        // Set tenant context for the query
        Tenant tenant;
        try
        {
            tenant = await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);
        }
        catch (InvalidOperationException)
        {
            return Result<PublicTrackingDto>.Fail("Invalid tracking link.");
        }

        // Find the tracking link
        var trackingLink = await tenantUow.Repository<TrackingLink>()
            .GetAsync(t => t.Token == req.Token, ct);

        if (trackingLink is null)
        {
            return Result<PublicTrackingDto>.Fail("Tracking link not found.");
        }

        if (!trackingLink.IsValid)
        {
            return Result<PublicTrackingDto>.Fail("This tracking link has expired or been revoked.");
        }

        // Update access tracking
        trackingLink.AccessCount++;
        trackingLink.LastAccessedAt = DateTime.UtcNow;
        tenantUow.Repository<TrackingLink>().Update(trackingLink);
        await tenantUow.SaveChangesAsync(ct);

        // Get the load
        var load = trackingLink.Load;
        if (load is null)
        {
            return Result<PublicTrackingDto>.Fail("Load not found.");
        }

        var dto = new PublicTrackingDto
        {
            LoadNumber = load.Number,
            LoadName = load.Name,
            Status = load.Status,
            OriginAddress = load.OriginAddress,
            DestinationAddress = load.DestinationAddress,
            CurrentAddress = load.AssignedTruck?.CurrentAddress,
            CurrentLocation = load.AssignedTruck?.CurrentLocation,
            DispatchedAt = load.DispatchedAt,
            PickedUpAt = load.PickedUpAt,
            DeliveredAt = load.DeliveredAt,
            DriverName =
                load.AssignedTruck != null ? string.Join(", ", load.AssignedTruck.GetDriversNames()) : null,
            TruckNumber = load.AssignedTruck?.Number,
            DocumentCount = load.Documents.Count,
            HasProofOfDelivery = load.Documents.Any(d => d.Type == DocumentType.ProofOfDelivery),
            HasBillOfLading = load.Documents.Any(d => d.Type == DocumentType.BillOfLading),
            TenantName = tenant.CompanyName ?? tenant.Name,
            TenantLogoUrl = GetLogoUrl(tenant)
        };

        return Result<PublicTrackingDto>.Ok(dto);
    }

    private string? GetLogoUrl(Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.LogoPath))
        {
            return null;
        }

        // If it's already a full URL (e.g., Azure Blob), return as-is
        if (tenant.LogoPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return tenant.LogoPath;
        }

        return blobStorageService.GetPublicUrl(BlobConstants.LogosContainerName, tenant.LogoPath, tenant.Id);
    }
}
