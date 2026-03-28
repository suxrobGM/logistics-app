using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CalculateAssignmentMetricsTool(ITenantUnitOfWork tenantUow) : IDispatchTool
{
    private const double KmToMiles = 0.621371;

    public string Name => "calculate_assignment_metrics";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var candidateNodes = input["candidates"]?.AsArray();
        if (candidateNodes is null || candidateNodes.Count == 0)
            return JsonSerializer.Serialize(new { error = "Missing or empty candidates array" });

        var results = new List<object>();

        foreach (var node in candidateNodes)
        {
            if (!Guid.TryParse(node?["load_id"]?.GetValue<string>(), out var loadId) ||
                !Guid.TryParse(node?["truck_id"]?.GetValue<string>(), out var truckId))
            {
                results.Add(new { error = "Invalid load_id or truck_id", load_id = node?["load_id"]?.ToString(), truck_id = node?["truck_id"]?.ToString() });
                continue;
            }

            var load = await tenantUow.Repository<Load>().GetByIdAsync(loadId, ct);
            var truck = await tenantUow.Repository<Truck>().GetByIdAsync(truckId, ct);

            if (load is null || truck is null)
            {
                results.Add(new { load_id = loadId, truck_id = truckId, error = load is null ? "Load not found" : "Truck not found" });
                continue;
            }

            // Calculate deadhead distance (truck current location → load pickup) using GeoPoint.DistanceTo()
            var deadheadKm = 0.0;
            if (truck.CurrentLocation is not null && load.OriginLocation is not null)
                deadheadKm = truck.CurrentLocation.DistanceTo(load.OriginLocation) / 1000.0;

            // Load distance (origin → destination) is stored in meters
            var loadedKm = load.Distance / 1000.0;
            var totalMiles = (deadheadKm + loadedKm) * KmToMiles;
            var deadheadMiles = deadheadKm * KmToMiles;
            var loadedMiles = loadedKm * KmToMiles;
            var deliveryCost = (double)(load.DeliveryCost?.Amount ?? 0);

            var revenuePerMile = totalMiles > 0 ? deliveryCost / totalMiles : 0;
            var deadheadRatio = totalMiles > 0 ? deadheadMiles / totalMiles : 0;

            results.Add(new
            {
                load_id = loadId,
                truck_id = truckId,
                load_name = load.Name,
                truck_number = truck.Number,
                deadhead_miles = Math.Round(deadheadMiles, 1),
                loaded_miles = Math.Round(loadedMiles, 1),
                total_miles = Math.Round(totalMiles, 1),
                delivery_cost = deliveryCost,
                revenue_per_mile = Math.Round(revenuePerMile, 2),
                deadhead_ratio = Math.Round(deadheadRatio, 3)
            });
        }

        // Sort by revenue per mile descending
        var sorted = results
            .OrderByDescending(r =>
            {
                var json = JsonSerializer.SerializeToElement(r);
                return json.TryGetProperty("revenue_per_mile", out var rpm) ? rpm.GetDouble() : -1;
            })
            .ToList();

        return JsonSerializer.Serialize(new { candidates = sorted, count = sorted.Count });
    }
}
