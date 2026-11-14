using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Portal.Queries;

internal sealed class GetPortalLoadsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetPortalLoadsQuery, PagedResult<PortalLoadDto>>
{
    private static readonly SortFields<Load> Sorts = new SortFields<Load>("-createdAt")
        .Add("number", l => l.Number)
        .Add("status", l => l.Status)
        .Add("createdAt", l => l.CreatedAt);

    public async Task<PagedResult<PortalLoadDto>> Handle(
        GetPortalLoadsQuery req,
        CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<Load>().Query()
            .Where(l => l.CustomerId == req.CustomerId);

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.ToLower();
            baseQuery = baseQuery.Where(l =>
                l.Name.ToLower().Contains(search) ||
                l.Number.ToString().Contains(search));
        }

        if (req.ActiveOnly)
        {
            baseQuery = baseQuery.Where(l => l.DeliveredAt == null && l.CancelledAt == null);
        }

        if (req.FromDate is { } fromDate)
        {
            baseQuery = baseQuery.Where(l => l.CreatedAt >= fromDate);
        }

        if (req.ToDate is { } toDate)
        {
            baseQuery = baseQuery.Where(l => l.CreatedAt <= toDate);
        }

        var totalItems = await baseQuery.CountAsync(ct);

        // Materialized before projecting: GetDriversNames can't be translated to SQL.
        var loadEntities = await Sorts.Apply(baseQuery, req.OrderBy, l => l.Id)
            .ApplyPaging(req.Page, req.PageSize)
            .ToListAsync(ct);

        var loads = loadEntities.Select(l => new PortalLoadDto
        {
            Id = l.Id,
            Number = l.Number,
            Name = l.Name,
            Status = l.Status,
            OriginAddress = l.OriginAddress,
            DestinationAddress = l.DestinationAddress,
            CurrentAddress = l.AssignedTruck?.CurrentAddress,
            CurrentLocation = l.AssignedTruck?.CurrentLocation,
            DispatchedAt = l.DispatchedAt,
            PickedUpAt = l.PickedUpAt,
            DeliveredAt = l.DeliveredAt,
            CreatedAt = l.CreatedAt,
            DeliveryCost = l.DeliveryCost,
            Distance = l.Distance,
            DriverName = l.AssignedTruck != null ? string.Join(", ", l.AssignedTruck.GetDriversNames()) : null,
            TruckNumber = l.AssignedTruck?.Number,
            DocumentCount = l.Documents.Count,
            HasProofOfDelivery = l.Documents.Any(d => d.Type == DocumentType.ProofOfDelivery),
            HasBillOfLading = l.Documents.Any(d => d.Type == DocumentType.BillOfLading)
        }).ToArray();

        return PagedResult<PortalLoadDto>.Ok(loads, totalItems, req.PageSize);
    }
}
