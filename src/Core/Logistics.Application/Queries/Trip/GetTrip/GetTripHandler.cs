using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTripHandler : IAppRequestHandler<GetTripQuery, Result<TripDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetTripHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result<TripDto>> Handle(
        GetTripQuery req, CancellationToken ct)
    {
        var trip = await _tenantUow.Repository<Trip>().GetByIdAsync(req.TripId);

        if (trip is null)
        {
            return Result<TripDto>.Fail($"Could not find a trip with ID '{req.TripId}'");
        }

        var tripeDto = trip.ToDto();
        return Result<TripDto>.Succeed(tripeDto);
    }
}
