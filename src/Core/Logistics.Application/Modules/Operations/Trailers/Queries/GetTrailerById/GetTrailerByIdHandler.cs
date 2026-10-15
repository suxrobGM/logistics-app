using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trailers.Queries;

internal sealed class GetTrailerByIdHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTrailerByIdQuery, Result<TrailerDto>>
{
    public async Task<Result<TrailerDto>> Handle(GetTrailerByIdQuery req, CancellationToken ct)
    {
        var trailer = await tenantUow.Repository<Trailer>().GetByIdAsync(req.Id, ct);
        if (trailer is null)
        {
            return Result<TrailerDto>.Fail($"Could not find a trailer with ID '{req.Id}'");
        }

        var truck = await tenantUow.Repository<Truck>().GetAsync(i => i.CurrentTrailerId == trailer.Id, ct);
        return Result<TrailerDto>.Ok(trailer.ToDto(truck));
    }
}
