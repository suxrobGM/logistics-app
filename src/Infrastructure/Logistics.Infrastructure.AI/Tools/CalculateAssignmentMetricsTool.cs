using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CalculateAssignmentMetricsTool(ITenantUnitOfWork tenantUow) : IAIDispatchTool
{
    private const double KmToMiles = 0.621371;
    private const double MetersPerKm = 1000.0;

    /// <summary>
    /// One scored truck/load pairing. A record rather than an anonymous type so the sort can read
    /// <see cref="RevenuePerMile"/> directly - the keys are snake_case because the model reads them
    /// by name, hence the explicit attributes.
    /// </summary>
    private sealed record CandidateMetric(
        [property: JsonPropertyName("load_id")] Guid LoadId,
        [property: JsonPropertyName("truck_id")] Guid TruckId,
        [property: JsonPropertyName("load_name")] string? LoadName,
        [property: JsonPropertyName("truck_number")] string TruckNumber,
        [property: JsonPropertyName("deadhead_miles")] double DeadheadMiles,
        [property: JsonPropertyName("loaded_miles")] double LoadedMiles,
        [property: JsonPropertyName("total_miles")] double TotalMiles,
        [property: JsonPropertyName("delivery_cost")] double DeliveryCost,
        [property: JsonPropertyName("revenue_per_mile")] double RevenuePerMile,
        [property: JsonPropertyName("deadhead_ratio")] double DeadheadRatio);

    public string Name => "calculate_assignment_metrics";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var candidateNodes = input["candidates"]?.AsArray();
        if (candidateNodes is null || candidateNodes.Count == 0)
            return ToolResult.Error("Missing or empty candidates array");

        var parsed = new List<(Guid LoadId, Guid TruckId)>();
        var errors = new List<object>();

        foreach (var node in candidateNodes)
        {
            if (!Guid.TryParse(node?["load_id"]?.GetValue<string>(), out var loadId) ||
                !Guid.TryParse(node?["truck_id"]?.GetValue<string>(), out var truckId))
            {
                errors.Add(new
                {
                    error = "Invalid load_id or truck_id",
                    load_id = node?["load_id"]?.ToString(),
                    truck_id = node?["truck_id"]?.ToString()
                });
                continue;
            }

            parsed.Add((loadId, truckId));
        }

        // The prompt tells the agent to score every competing pairing at once, so a per-candidate
        // GetByIdAsync pair meant 2N sequential round trips. Two batched reads instead.
        var loadIds = parsed.Select(p => p.LoadId).Distinct().ToList();
        var truckIds = parsed.Select(p => p.TruckId).Distinct().ToList();

        var loads = loadIds.Count > 0
            ? (await tenantUow.Repository<Load>().GetListAsync(l => loadIds.Contains(l.Id), ct))
                .ToDictionary(l => l.Id)
            : [];
        var trucks = truckIds.Count > 0
            ? (await tenantUow.Repository<Truck>().GetListAsync(t => truckIds.Contains(t.Id), ct))
                .ToDictionary(t => t.Id)
            : [];

        var metrics = new List<CandidateMetric>();

        foreach (var (loadId, truckId) in parsed)
        {
            var load = loads.GetValueOrDefault(loadId);
            var truck = trucks.GetValueOrDefault(truckId);

            if (load is null || truck is null)
            {
                errors.Add(new
                {
                    load_id = loadId,
                    truck_id = truckId,
                    error = load is null ? "Load not found" : "Truck not found"
                });
                continue;
            }

            var deadheadKm = 0.0;
            if (truck.CurrentLocation is not null && load.OriginLocation is not null)
                deadheadKm = truck.CurrentLocation.DistanceTo(load.OriginLocation) / MetersPerKm;

            var loadedKm = load.Distance / MetersPerKm;
            var totalMiles = (deadheadKm + loadedKm) * KmToMiles;
            var deadheadMiles = deadheadKm * KmToMiles;
            var loadedMiles = loadedKm * KmToMiles;
            var deliveryCost = (double)(load.DeliveryCost?.Amount ?? 0);

            metrics.Add(new CandidateMetric(
                loadId,
                truckId,
                load.Name,
                truck.Number,
                Math.Round(deadheadMiles, 1),
                Math.Round(loadedMiles, 1),
                Math.Round(totalMiles, 1),
                deliveryCost,
                RevenuePerMile: Math.Round(totalMiles > 0 ? deliveryCost / totalMiles : 0, 2),
                DeadheadRatio: Math.Round(totalMiles > 0 ? deadheadMiles / totalMiles : 0, 3)));
        }

        // Best pairings first; unscoreable candidates trail them, as before.
        var candidates = metrics
            .OrderByDescending(m => m.RevenuePerMile)
            .Cast<object>()
            .Concat(errors)
            .ToList();

        return ToolResult.Ok(new { candidates, count = candidates.Count });
    }
}
