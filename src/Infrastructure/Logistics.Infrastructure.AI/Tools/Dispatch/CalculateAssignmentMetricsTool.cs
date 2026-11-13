using System.ComponentModel;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.ApplicationServices;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CalculateAssignmentMetricsTool(ITenantUnitOfWork tenantUow, IProfitEstimateService profitEstimate)
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
        "Score candidate truck-load pairs: deadhead, loaded and total distance, linehaul (delivery_cost, excluding fuel surcharge and accessorials), linehaul rate per distance unit, deadhead ratio, and est_margin with margin per distance unit (revenue minus truck operating cost over the total distance minus the driver's per-load pay; absent when costs are unknown). Best margin per distance first, else best rate. Distances are in the tenant's unit, named in each key (_mi or _km).")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (input.Candidates.Length == 0)
            return ToolResult.Error("Missing or empty candidates array");

        // Two batched reads, not a GetByIdAsync pair per candidate.
        var loadIds = input.Candidates.Select(c => c.LoadId).Distinct().ToList();
        var truckIds = input.Candidates.Select(c => c.TruckId).Distinct().ToList();

        var loads = (await tenantUow.Repository<Load>().GetListAsync(l => loadIds.Contains(l.Id), ct))
            .ToDictionary(l => l.Id);
        var trucks = (await tenantUow.Repository<Truck>().GetListAsync(t => truckIds.Contains(t.Id), ct))
            .ToDictionary(t => t.Id);

        var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;
        var suffix = unit.Abbreviation();

        var scored = new List<(double? MarginPer, double Rate, Dictionary<string, object?> Row)>();
        var pairs = new List<(Candidate Candidate, Load Load, Truck Truck, double DeadheadMeters)>();
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

            var deadheadMeters = truck.CurrentLocation is not null && load.OriginLocation is not null
                ? truck.CurrentLocation.DistanceTo(load.OriginLocation)
                : 0.0;
            pairs.Add((candidate, load, truck, deadheadMeters));
        }

        var margins = await profitEstimate.EstimateMarginsAsync(
            pairs.Select(p => new MarginRequest(p.Truck.Id, p.Load.Id, p.DeadheadMeters + p.Load.Distance)).ToList(), ct);

        foreach (var (candidate, load, truck, deadheadMeters) in pairs)
        {
            var deadhead = DispatchUnits.FromMeters(deadheadMeters, unit);
            var total = DispatchUnits.FromMeters(deadheadMeters + load.Distance, unit);
            var linehaul = load.DeliveryCost?.Amount ?? 0;
            var rate = DispatchUnits.RatePer(linehaul, total);

            // Keys carry the unit so the model never has to guess whether to convert.
            var row = new Dictionary<string, object?>
            {
                ["load_id"] = candidate.LoadId,
                ["truck_id"] = candidate.TruckId,
                ["load_name"] = load.Name,
                ["truck_number"] = truck.Number,
                [$"deadhead_{suffix}"] = deadhead,
                [$"loaded_{suffix}"] = DispatchUnits.FromMeters(load.Distance, unit),
                [$"total_{suffix}"] = total,
                ["delivery_cost"] = linehaul,
                [$"revenue_per_{suffix}"] = rate,
                ["deadhead_ratio"] = Math.Round(total > 0 ? deadhead / total : 0, 3)
            };
            decimal? margin = margins.TryGetValue((truck.Id, load.Id), out var m) ? m : null;
            DispatchUnits.AddMargin(row, margin, total, unit);
            scored.Add((DispatchUnits.MarginPer(margin, total), rate, row));
        }

        // Best pairings first; unscoreable candidates last.
        var candidates = scored
            .OrderByDescending(s => s.MarginPer ?? double.MinValue)
            .ThenByDescending(s => s.Rate)
            .Select(s => (object)s.Row)
            .Concat(errors)
            .ToList();

        return ToolResult.Ok(new { candidates, count = candidates.Count });
    }
}
