using Logistics.Application.Services;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Services.Trip;

public sealed class HeuristicTripOptimizer : ITripOptimizer
{
    public Task<IReadOnlyList<TripStopDto>> OptimizeAsync(OptimizeTripParams @params, CancellationToken ct = default)
    {
        var pickups = @params.Stops.Where(s => s.Type == TripStopType.PickUp).ToList();
        var dropoffs = @params.Stops.Where(s => s.Type == TripStopType.DropOff).ToList();

        var ordered = new List<TripStopDto>(@params.Stops.Count);
        var cur = @params.Start ?? pickups.First().Location;
        var capacity = 0;

        // pick-ups with capacity guard, interleave earliest possible drop when full
        while (pickups.Count > 0)
        {
            // choose feasible pickup closest to current
            TripStopDto? nextPickup = null;
            var best = double.MaxValue;
            foreach (var p in pickups)
            {
                if (capacity + p.GetDemand() > @params.VehicleCapacity)
                {
                    continue; // demand is +Units
                }

                var d = cur.DistanceTo(p.Location);
                if (d < best)
                {
                    best = d;
                    nextPickup = p;
                }
            }

            if (nextPickup is not null)
            {
                ordered.Add(nextPickup);
                capacity += nextPickup.GetDemand();
                cur = nextPickup.Location;
                pickups.Remove(nextPickup);
            }
            else
            {
                // full: drop the closest eligible drop (for a load that is on board)
                var eligible = dropoffs.Where(d => ordered.Any(o => o.LoadId == d.LoadId)).ToList();
                if (eligible.Count == 0)
                {
                    // no legal move => bail: append any remaining (keeps function total)
                    ordered.AddRange(pickups);
                    ordered.AddRange(dropoffs);
                    break;
                }

                var drop = eligible.MinBy(d => cur.DistanceTo(d.Location))!;
                ordered.Add(drop);
                capacity += drop.GetDemand(); // negative
                cur = drop.Location;
                dropoffs.Remove(drop);
            }
        }

        // finish remaining drops by NN while keeping precedence
        while (dropoffs.Count > 0)
        {
            var eligible = dropoffs.Where(d => ordered.Any(o => o.LoadId == d.LoadId)).ToList();
            var next = (eligible.Count > 0 ? eligible : dropoffs).MinBy(d => cur.DistanceTo(d.Location))!;
            ordered.Add(next);
            capacity += next.GetDemand();
            cur = next.Location;
            dropoffs.Remove(next);
        }

        // re-number orders
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Order = i + 1;
        }

        return Task.FromResult<IReadOnlyList<TripStopDto>>(ordered);
    }
}
