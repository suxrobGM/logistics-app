using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trips.Queries;

internal sealed class GetTripHandler(ITenantUnitOfWork tenantUow) : IRequestHandler<GetTripQuery, Result<TripDto>>
{
    public async Task<Result<TripDto>> Handle(
        GetTripQuery req, CancellationToken ct)
    {
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(req.TripId);

        if (trip is null)
        {
            return Result<TripDto>.Fail($"Could not find a trip with ID '{req.TripId}'");
        }

        var tripeDto = trip.ToDto(trip.Trailer?.Number);
        return Result<TripDto>.Ok(tripeDto);
    }
}
