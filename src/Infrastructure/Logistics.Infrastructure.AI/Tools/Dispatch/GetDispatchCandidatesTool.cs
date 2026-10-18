using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

/// <summary>
/// Ranks the Available trucks for each unassigned load in one call. Everything the prompt used to
/// ask the model to work out by hand - the truck-type rule, the HOS arithmetic, deadhead, trailer
/// fit, rate per mile - runs here, on the same rules the write tools enforce.
/// </summary>
internal sealed class GetDispatchCandidatesTool(
    IMediator mediator,
    ITenantUnitOfWork tenantUow,
    IDispatchEligibilityService eligibilityService)
    : AgentTool<GetDispatchCandidatesTool.Input>, IAgentToolMetadata
{
    private const int DefaultLimit = 3;
    private const int MaxLimit = 10;

    internal sealed record Input
    {
        [Description("Load IDs (GUIDs) to plan. Omit to plan every unassigned load.")]
        public Guid[]? LoadIds { get; init; }

        [Description("Best candidates to return per load (default 3, max 10).")]
        public int? Limit { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_dispatch_candidates",
        "Rank the Available trucks for each unassigned load. Type-incompatible trucks and drivers whose hours cannot cover the trip are already removed and counted under excluded. Each candidate carries deadhead and total distance in the tenant's unit (key suffix _mi or _km), estimated drive_time and the driver's driving_time_left, hos (feasible, multi_day or unknown), equipment (match, unconfirmed or mismatch, with a note) and linehaul rate per distance unit. Hazmat and car hauler loads also run the eligibility check: blocked = true means an error issue that stops the assignment. Best candidate first.")
    {
        RequiredPermission = Permission.Dispatch.View,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnassignedLoadsQuery { PageSize = ToolResult.MaxUnassignedLoads }, ct);
        if (!result.IsSuccess)
            return ToolResult.Error(result.Error ?? "Unknown error");

        var loads = (result.Value ?? [])
            .Where(l => input.LoadIds is not { Length: > 0 } ids || ids.Contains(l.Id))
            .ToList();

        var fleet = await AvailableFleet.LoadAsync(tenantUow, ct);
        var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;
        var limit = Math.Clamp(input.Limit ?? DefaultLimit, 1, MaxLimit);

        var planned = new List<object>(loads.Count);
        foreach (var load in loads)
        {
            planned.Add(await PlanAsync(load, fleet, unit, limit, ct));
        }

        return ToolResult.Ok(new
        {
            loads = planned,
            count = planned.Count,
            total_unassigned = result.TotalItems,
            available_trucks = fleet.Count
        });
    }

    private async Task<Dictionary<string, object?>> PlanAsync(
        LoadDto load, List<AvailableTruck> fleet, DistanceUnit unit, int limit, CancellationToken ct)
    {
        var suffix = DispatchUnits.Suffix(unit);
        var loadedKm = DispatchUnits.MetersToKm(load.Distance);
        var (typeIncompatible, hosInfeasible, noDriver) = (0, 0, 0);
        var scored = new List<Scored>();

        foreach (var entry in fleet)
        {
            if (!TruckLoadCompatibility.CanHaul(entry.Truck.Type, load.Type))
            {
                typeIncompatible++;
                continue;
            }

            if (entry.Driver is null)
            {
                noDriver++;
                continue;
            }

            double? deadheadKm = entry.Truck.CurrentLocation is { } at
                ? DispatchUnits.MetersToKm(at.DistanceTo(load.OriginLocation))
                : null;
            var tripKm = (deadheadKm ?? 0) + loadedKm;

            var verdict = entry.Hos is null ? HosFeasibility.Unknown(tripKm) : HosFeasibility.Evaluate(entry.Hos, tripKm);
            if (entry.Hos is not null && !verdict.Feasible && !verdict.FeasibleMultiDay)
            {
                hosInfeasible++;
                continue;
            }

            var hos = HosLabel(entry.Hos is not null, verdict);
            var (equipment, note) = EquipmentFit.Evaluate(load.Type, entry.Truck.Type, entry.Trailer?.Type);
            scored.Add(new Scored(entry, deadheadKm, tripKm, verdict, hos, equipment, note));
        }

        var best = scored
            .OrderBy(s => s.Equipment)
            .ThenBy(s => s.Hos switch { "feasible" => 0, "multi_day" => 1, _ => 2 })
            .ThenBy(s => s.DeadheadKm ?? double.MaxValue)
            .Take(limit)
            .ToList();

        var needsEligibility = AgentHazmat.DescribeClass(load) is not null || load.Vehicles.Count > 0;
        var candidates = new List<(bool Blocked, Dictionary<string, object?> Row)>(best.Count);
        foreach (var candidate in best)
        {
            var row = Describe(candidate, load, unit, suffix);
            var blocked = false;

            if (needsEligibility)
            {
                var eligibility = await eligibilityService.CheckAsync(candidate.Entry.Truck.Id, load.Id, ct: ct);
                blocked = !eligibility.IsEligible;
                row["blocked"] = blocked;
                row["issues"] = eligibility.Issues.Select(i => i.Message).ToArray();
            }

            candidates.Add((blocked, row));
        }

        return new Dictionary<string, object?>
        {
            ["load_id"] = load.Id,
            ["load_name"] = load.Name,
            ["load_type"] = load.Type.ToString(),
            [$"loaded_{suffix}"] = DispatchUnits.FromKm(loadedKm, unit),
            ["linehaul"] = load.DeliveryCost,
            // Blocked candidates trail the rest without disturbing their order.
            ["candidates"] = candidates.OrderBy(c => c.Blocked).Select(c => c.Row).ToList(),
            ["excluded"] = new
            {
                type_incompatible = typeIncompatible,
                hos_infeasible = hosInfeasible,
                no_driver = noDriver
            }
        };
    }

    private static Dictionary<string, object?> Describe(Scored candidate, LoadDto load, DistanceUnit unit, string suffix)
    {
        var (truck, driver, hos, trailer) = candidate.Entry;
        var total = DispatchUnits.FromKm(candidate.TripKm, unit);

        return new Dictionary<string, object?>
        {
            ["truck_id"] = truck.Id,
            ["truck_number"] = truck.Number,
            ["truck_type"] = truck.Type.ToString(),
            ["trailer_type"] = trailer?.Type.ToString(),
            ["driver_id"] = driver!.Id,
            ["driver_name"] = driver.GetFullName(),
            [$"deadhead_{suffix}"] = candidate.DeadheadKm is { } km ? DispatchUnits.FromKm(km, unit) : null,
            [$"total_{suffix}"] = total,
            ["drive_time"] = AgentDuration.Format(candidate.Verdict.EstimatedDrivingMinutes),
            ["driving_time_left"] = hos is null ? null : AgentDuration.Format(hos.DrivingMinutesRemaining),
            ["hos"] = candidate.Hos,
            ["equipment"] = candidate.Equipment.ToString().ToLowerInvariant(),
            ["equipment_note"] = candidate.EquipmentNote,
            [$"revenue_per_{suffix}"] = total > 0 ? Math.Round((double)load.DeliveryCost / total, 2) : 0
        };
    }

    private static string HosLabel(bool hasHosData, HosFeasibility verdict)
    {
        if (!hasHosData)
            return "unknown";

        return verdict.Feasible ? "feasible" : "multi_day";
    }

    private sealed record Scored(
        AvailableTruck Entry,
        double? DeadheadKm,
        double TripKm,
        HosFeasibility Verdict,
        string Hos,
        EquipmentMatch Equipment,
        string? EquipmentNote);
}
