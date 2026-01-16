using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetPortalLoadsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetPortalLoadsQuery, PagedResult<PortalLoadDto>>
{
    public async Task<PagedResult<PortalLoadDto>> Handle(
        GetPortalLoadsQuery req,
        CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<Load>().Query()
            .Where(l => l.CustomerId == req.CustomerId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.ToLower();
            baseQuery = baseQuery.Where(l =>
                l.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                l.Number.ToString().Contains(search));
        }

        // Apply active loads filter
        if (req.OnlyActiveLoads)
        {
            baseQuery = baseQuery.Where(l => l.DeliveredAt == null && l.CancelledAt == null);
        }

        // Apply date filters
        if (req is { StartDate: not null, EndDate: not null })
        {
            baseQuery = baseQuery.Where(l => l.CreatedAt >= req.StartDate && l.CreatedAt <= req.EndDate);
        }

        var totalItems = await baseQuery.CountAsync(ct);

        // Apply ordering
        baseQuery = req.OrderBy?.ToLower() switch
        {
            "number" => baseQuery.OrderBy(l => l.Number),
            "number_desc" => baseQuery.OrderByDescending(l => l.Number),
            "status" => baseQuery.OrderBy(l => l.Status),
            "created" => baseQuery.OrderBy(l => l.CreatedAt),
            _ => baseQuery.OrderByDescending(l => l.CreatedAt)
        };

        // Apply paging
        baseQuery = baseQuery.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize);

        // Load data first, then project (GetDriversNames can't be translated to SQL)
        var loadEntities = await baseQuery
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

        return PagedResult<PortalLoadDto>.Succeed(loads, totalItems, req.PageSize);
    }
}
