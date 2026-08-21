using System.ComponentModel;
using System.Text.Json.Serialization;
using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CalculateAssignmentMetricsTool(ITenantUnitOfWork tenantUow)
    : AgentTool<CalculateAssignmentMetricsTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Array of truck/load pairs to evaluate")]
        public required Candidate[] Candidates { get; init; }
    }

    internal sealed record Candidate
    {
        [Description("The load ID (GUID)")]
        public required Guid LoadId { get; init; }

        [Description("The truck ID (GUID)")]
        public required Guid TruckId { get; init; }
    }

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

    public static AgentToolDefinition Definition => new(
        "calculate_assignment_metrics",
        "Calculate revenue per mile/km, deadhead ratio, and profitability for candidate truck-load pairs. Use this when multiple trucks are candidates for a load to pick the most profitable option.")
    {
        RequiredPermission = Permission.Dispatch.View,
        DispatchAgent = true
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (input.Candidates.Length == 0)
            return ToolResult.Error("Missing or empty candidates array");

        // The prompt tells the agent to score every competing pairing at once, so a per-candidate
        // GetByIdAsync pair meant 2N sequential round trips. Two batched reads instead.
        var loadIds = input.Candidates.Select(c => c.LoadId).Distinct().ToList();
        var truckIds = input.Candidates.Select(c => c.TruckId).Distinct().ToList();

        var loads = (await tenantUow.Repository<Load>().GetListAsync(l => loadIds.Contains(l.Id), ct))
            .ToDictionary(l => l.Id);
        var trucks = (await tenantUow.Repository<Truck>().GetListAsync(t => truckIds.Contains(t.Id), ct))
            .ToDictionary(t => t.Id);

        var metrics = new List<CandidateMetric>();
        var errors = new List<object>();

        foreach (var candidate in input.Candidates)
        {
            var load = loads.GetValueOrDefault(candidate.LoadId);
            var truck = trucks.GetValueOrDefault(candidate.TruckId);

            if (load is null || truck is null)
            {
                errors.Add(new
                {
                    load_id = candidate.LoadId,
                    truck_id = candidate.TruckId,
                    error = load is null ? "Load not found" : "Truck not found"
                });
                continue;
            }

            var deadheadKm = 0.0;
            if (truck.CurrentLocation is not null && load.OriginLocation is not null)
                deadheadKm = DispatchUnits.MetersToKm(truck.CurrentLocation.DistanceTo(load.OriginLocation));

            var loadedKm = DispatchUnits.MetersToKm(load.Distance);
            var totalMiles = DispatchUnits.KmToMiles(deadheadKm + loadedKm);
            var deadheadMiles = DispatchUnits.KmToMiles(deadheadKm);
            var loadedMiles = DispatchUnits.KmToMiles(loadedKm);
            var deliveryCost = (double)(load.DeliveryCost?.Amount ?? 0);

            metrics.Add(new CandidateMetric(
                candidate.LoadId,
                candidate.TruckId,
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
