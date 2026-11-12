using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Operations.Tracking.Commands;

namespace Logistics.Application.Modules.Operations.Tracking.Queries;

internal sealed class GetPublicTrackingHandler(
    ITrackingLinkService trackingLinkService,
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorageService,
    ICommandEnqueuer commandEnqueuer)
    : IRequestHandler<GetPublicTrackingQuery, Result<PublicTrackingDto>>
{
    public async Task<Result<PublicTrackingDto>> Handle(
        GetPublicTrackingQuery req,
        CancellationToken ct)
    {
        var opened = await trackingLinkService.OpenPublicLinkAsync(req.TenantId, req.Token, ct);
        if (!opened.IsSuccess)
        {
            return Result<PublicTrackingDto>.Fail(opened.Error!);
        }

        var trackingLink = opened.Value!;
        var tenant = tenantUow.GetCurrentTenant();

        // Defer access bookkeeping so the read path stays write-free.
        commandEnqueuer.Enqueue(new RecordTrackingAccessCommand
        {
            TenantId = req.TenantId,
            TrackingLinkId = trackingLink.Id
        });

        // Get the load
        var load = trackingLink.Load;
        if (load is null)
        {
            return Result<PublicTrackingDto>.Fail("Load not found.", ErrorCodes.NotFound);
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
            TenantName = tenant.DisplayName,
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
