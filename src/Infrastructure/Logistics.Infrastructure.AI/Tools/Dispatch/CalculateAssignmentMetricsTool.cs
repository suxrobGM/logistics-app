using System.ComponentModel;
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
        [AgentEntityId(AgentEntityKind.Load)]
        public required Guid LoadId { get; init; }

        [Description("The truck ID (GUID)")]
        [AgentEntityId(AgentEntityKind.Truck)]
        public required Guid TruckId { get; init; }
    }


    public static AgentToolDefinition Definition => new(
        "calculate_assignment_metrics",
        "Score candidate truck-load pairs: deadhead, loaded and total distance, linehaul (delivery_cost, excluding fuel surcharge and accessorials), linehaul rate per distance unit, and deadhead ratio, best rate first. Distances are in the tenant's unit, named in each key (_mi or _km).")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
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

        var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;
        var suffix = DispatchUnits.Suffix(unit);

        var scored = new List<(double Rate, Dictionary<string, object?> Row)>();
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

            var deadheadKm = truck.CurrentLocation is not null && load.OriginLocation is not null
                ? DispatchUnits.MetersToKm(truck.CurrentLocation.DistanceTo(load.OriginLocation))
                : 0.0;
            var loadedKm = DispatchUnits.MetersToKm(load.Distance);

            var deadhead = DispatchUnits.FromKm(deadheadKm, unit);
            var total = DispatchUnits.FromKm(deadheadKm + loadedKm, unit);
            var linehaul = (double)(load.DeliveryCost?.Amount ?? 0);
            var rate = Math.Round(total > 0 ? linehaul / total : 0, 2);

            // Keys carry the unit so the model never has to guess whether to convert.
            scored.Add((rate, new Dictionary<string, object?>
            {
                ["load_id"] = candidate.LoadId,
                ["truck_id"] = candidate.TruckId,
                ["load_name"] = load.Name,
                ["truck_number"] = truck.Number,
                [$"deadhead_{suffix}"] = deadhead,
                [$"loaded_{suffix}"] = DispatchUnits.FromKm(loadedKm, unit),
                [$"total_{suffix}"] = total,
                ["delivery_cost"] = linehaul,
                [$"revenue_per_{suffix}"] = rate,
                ["deadhead_ratio"] = Math.Round(total > 0 ? deadhead / total : 0, 3)
            }));
        }

        // Best pairings first; unscoreable candidates trail them, as before.
        var candidates = scored
            .OrderByDescending(s => s.Rate)
            .Select(s => (object)s.Row)
            .Concat(errors)
            .ToList();

        return ToolResult.Ok(new { candidates, count = candidates.Count });
    }
}
