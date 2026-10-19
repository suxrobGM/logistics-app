using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trailers.Queries;

internal sealed class GetTrailersHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTrailersQuery, PagedResult<TrailerDto>>
{
    public async Task<PagedResult<TrailerDto>> Handle(GetTrailersQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Trailer>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i =>
                i.Number.ToLower().Contains(search) ||
                (i.Vin != null && i.Vin.ToLower().Contains(search)) ||
                (i.LicensePlate != null && i.LicensePlate.ToLower().Contains(search)));
        }

        if (req.Status.HasValue)
        {
            query = query.Where(i => i.Status == req.Status.Value);
        }

        if (req.Type.HasValue)
        {
            query = query.Where(i => i.Type == req.Type.Value);
        }

        var totalItems = await query.CountAsync(ct);
        var trailers = await query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .ToArrayAsync(ct);

        var trailerIds = trailers.Select(i => i.Id).ToArray();
        var trucksByTrailer = await tenantUow.Repository<Truck>().Query()
            .Where(i => i.CurrentTrailerId != null && trailerIds.Contains(i.CurrentTrailerId.Value))
            .ToDictionaryAsync(i => i.CurrentTrailerId!.Value, ct);

        var items = trailers.Select(i => i.ToDto(trucksByTrailer.GetValueOrDefault(i.Id))).ToArray();
        return PagedResult<TrailerDto>.Ok(items, totalItems, req.PageSize);
    }
}
