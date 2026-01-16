using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetPortalLoadHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetPortalLoadQuery, Result<PortalLoadDto>>
{
    public async Task<Result<PortalLoadDto>> Handle(
        GetPortalLoadQuery req,
        CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().Query()
            .Where(l => l.Id == req.LoadId && l.CustomerId == req.CustomerId)
            .FirstOrDefaultAsync(ct);

        if (load == null)
        {
            return Result<PortalLoadDto>.Fail("Load not found or access denied.");
        }

        var dto = new PortalLoadDto
        {
            Id = load.Id,
            Number = load.Number,
            Name = load.Name,
            Status = load.Status,
            OriginAddress = load.OriginAddress,
            DestinationAddress = load.DestinationAddress,
            CurrentAddress = load.AssignedTruck?.CurrentAddress,
            CurrentLocation = load.AssignedTruck?.CurrentLocation,
            DispatchedAt = load.DispatchedAt,
            PickedUpAt = load.PickedUpAt,
            DeliveredAt = load.DeliveredAt,
            CreatedAt = load.CreatedAt,
            DeliveryCost = load.DeliveryCost,
            Distance = load.Distance,
            DriverName = load.AssignedTruck != null ? string.Join(", ", load.AssignedTruck.GetDriversNames()) : null,
            TruckNumber = load.AssignedTruck?.Number,
            DocumentCount = load.Documents.Count,
            HasProofOfDelivery = load.Documents.Any(d => d.Type == DocumentType.ProofOfDelivery),
            HasBillOfLading = load.Documents.Any(d => d.Type == DocumentType.BillOfLading)
        };

        return Result<PortalLoadDto>.Ok(dto);
    }
}
