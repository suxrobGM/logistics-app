using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTripHandler : RequestHandler<GetTripQuery, Result<TripDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTripHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<TripDto>> HandleValidated(
        GetTripQuery req, CancellationToken ct)
    {
        var trip = await _tenantUow.Repository<Trip>().GetByIdAsync(req.TripId);

        if (trip is null) return Result<TripDto>.Fail($"Could not find a trip with ID '{req.TripId}'");

        var tripeDto = trip.ToDto();
        return Result<TripDto>.Succeed(tripeDto);
    }
}
