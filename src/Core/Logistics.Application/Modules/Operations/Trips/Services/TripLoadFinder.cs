using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trips.Services;

internal sealed class TripLoadFinder(ITenantUnitOfWork tenantUow) : ITripLoadFinder
{
    public async Task<Result<List<Load>>> FindAttachableAsync(
        IReadOnlyCollection<Guid> loadIds,
        Guid? tripId,
        CancellationToken ct)
    {
        if (loadIds.Count == 0)
        {
            return Result<List<Load>>.Ok([]);
        }

        var loads = await tenantUow.Repository<Load>().GetListAsync(l => loadIds.Contains(l.Id), ct);
        var missingId = loadIds.Except(loads.Select(l => l.Id)).FirstOrDefault();
        if (missingId != Guid.Empty)
        {
            return Result<List<Load>>.Fail($"Load '{missingId}' not found.", ErrorCodes.NotFound);
        }

        if (loads.FirstOrDefault(l => l.Status is LoadStatus.Delivered or LoadStatus.Cancelled) is { } closed)
        {
            return Result<List<Load>>.Fail(
                $"Load #{closed.Number} is {closed.Status.GetDescription().ToLowerInvariant()} and can't be added to a trip.");
        }

        var onOtherTripId = await tenantUow.Repository<TripStop>().Query()
            .Where(s => loadIds.Contains(s.LoadId) && s.TripId != tripId)
            .Select(s => s.LoadId)
            .FirstOrDefaultAsync(ct);
        if (onOtherTripId != Guid.Empty)
        {
            var number = loads.First(l => l.Id == onOtherTripId).Number;
            return Result<List<Load>>.Fail($"Load #{number} is already on another trip.");
        }

        return Result<List<Load>>.Ok(loads);
    }
}
