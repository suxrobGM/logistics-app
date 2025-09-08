using Logistics.Application.Services;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Services.Trip;

public sealed class MapboxMatrixTripOptimizer : ITripOptimizer
{
    private readonly MapboxMatrixClient _matrix;

    public MapboxMatrixTripOptimizer(MapboxMatrixClient matrix)
    {
        _matrix = matrix;
    }

    public async Task<IReadOnlyList<TripStopDto>> OptimizeAsync(OptimizeTripParams @params,
        CancellationToken ct = default)
    {
        // 1) Build coordinate list and index mapping
        var coords = new List<(double lng, double lat)>();
        var idxOf = new Dictionary<Guid, int>(); // stopId -> matrix index

        var startIdx = -1;

        if (@params.Start is not null)
        {
            startIdx = coords.Count;
            coords.Add((@params.Start.Longitude, @params.Start.Latitude));
        }

        foreach (var s in @params.Stops.OrderBy(s => s.Order))
        {
            idxOf[s.Id] = coords.Count;
            coords.Add((s.Location.Longitude, s.Location.Latitude));
        }

        if (@params.End is not null)
        {
            coords.Add((@params.End.Longitude, @params.End.Latitude));
        }

        // 2) Query Matrix
        var matrix = await _matrix.GetMatrixAsync(coords, ct);

        // 3) Greedy PD with capacity
        var remaining = @params.Stops.ToDictionary(s => s.Id, s => s);
        var pickedLoads = new HashSet<Guid>(); // LoadIds already picked
        var route = new List<TripStopDto>(@params.Stops.Count);

        // start from depot if exists else from first stop
        var curIdx = startIdx >= 0 ? startIdx : idxOf[@params.Stops[0].Id];
        var capacity = 0;

        while (remaining.Count > 0)
        {
            TripStopDto? best = null;
            var bestCost = double.PositiveInfinity;

            foreach (var s in remaining.Values)
            {
                // precedence
                if (s.Type == TripStopType.DropOff && !pickedLoads.Contains(s.LoadId))
                {
                    continue;
                }

                // capacity
                var nextCapacity = capacity + s.GetDemand();
                if (nextCapacity < 0 || nextCapacity > @params.VehicleCapacity)
                {
                    continue;
                }

                // cost from current to candidate
                var toIdx = idxOf[s.Id];
                var cost = matrix.Cost(curIdx, toIdx);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    best = s;
                }
            }

            if (best is null)
            {
                // infeasible with matrix-driven choice -> bail to caller/fallback
                throw new InvalidOperationException("No feasible next stop found (capacity/precedence block).");
            }

            // commit choice
            route.Add(best);
            capacity += best.GetDemand();
            if (best.Type == TripStopType.PickUp)
            {
                pickedLoads.Add(best.LoadId);
            }

            curIdx = idxOf[best.Id];
            remaining.Remove(best.Id);
        }

        // 4) Renumber orders
        for (var i = 0; i < route.Count; i++)
        {
            route[i].Order = i + 1;
        }

        return route;
    }
}
