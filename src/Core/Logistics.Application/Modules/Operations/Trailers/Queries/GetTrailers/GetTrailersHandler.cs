using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trailers.Queries;

internal sealed class GetTrailersHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTrailersQuery, PagedResult<TrailerDto>>
{
    private static readonly SortFields<Trailer> Sorts = new SortFields<Trailer>("number")
        .Add("number", x => x.Number);

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

        return await query.ToPagedResultAsync(req, Sorts, MapPageAsync, ct);
    }

    private async Task<IEnumerable<TrailerDto>> MapPageAsync(IReadOnlyList<Trailer> trailers, CancellationToken ct)
    {
        var trailerIds = trailers.Select(i => i.Id).ToArray();
        var trucksByTrailer = await tenantUow.Repository<Truck>().Query()
            .Where(i => i.CurrentTrailerId != null && trailerIds.Contains(i.CurrentTrailerId.Value))
            .ToDictionaryAsync(i => i.CurrentTrailerId!.Value, ct);

        return trailers.Select(i => i.ToDto(trucksByTrailer.GetValueOrDefault(i.Id)));
    }
}
