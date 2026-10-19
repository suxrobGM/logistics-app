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
/// Ranks the Available trucks for each unassigned load. The type rule, HOS, deadhead, trailer fit
/// and rate run here, on the rules the write tools enforce, so the model does no arithmetic.
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
        var query = new GetUnassignedLoadsQuery { PageSize = ToolResult.MaxUnassignedLoads, LoadIds = input.LoadIds };
        var result = await mediator.Send(query, ct);
        if (!result.IsSuccess)
            return ToolResult.Error(result.Error ?? "Unknown error");

        var loads = result.Value?.ToList() ?? [];

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

            double? deadheadMeters = entry.Truck.CurrentLocation is { } at ? at.DistanceTo(load.OriginLocation) : null;
            var tripMeters = (deadheadMeters ?? 0) + load.Distance;
            var tripKm = DispatchUnits.MetersToKm(tripMeters);

            HosFeasibility verdict;
            HosFit fit;
            if (entry.Hos is null)
            {
                verdict = HosFeasibility.Unknown(tripKm);
                fit = HosFit.Unknown;
            }
            else
            {
                verdict = HosFeasibility.Evaluate(entry.Hos, tripKm);
                if (!verdict.Feasible && !verdict.FeasibleMultiDay)
                {
                    hosInfeasible++;
                    continue;
                }

                fit = verdict.Feasible ? HosFit.Feasible : HosFit.MultiDay;
            }

            var (equipment, note) = EquipmentFit.Evaluate(load.Type, entry.Truck.Type, entry.Trailer?.Type);
            scored.Add(new Scored(entry, deadheadMeters, tripMeters, verdict, fit, equipment, note));
        }

        var best = scored
            .OrderBy(s => s.Equipment)
            .ThenBy(s => s.Hos)
            .ThenBy(s => s.DeadheadMeters ?? double.MaxValue)
            .Take(limit)
            .ToList();

        var needsEligibility = load.HazmatClass is not null || load.Vehicles.Count > 0;
        var candidates = new List<(bool Blocked, Dictionary<string, object?> Row)>(best.Count);
        foreach (var candidate in best)
        {
            var row = Describe(candidate, load, unit);
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
            [$"loaded_{unit.Abbreviation()}"] = DispatchUnits.FromMeters(load.Distance, unit),
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

    private static Dictionary<string, object?> Describe(Scored candidate, LoadDto load, DistanceUnit unit)
    {
        var (truck, driver, hos, trailer) = candidate.Entry;
        var suffix = unit.Abbreviation();
        var total = DispatchUnits.FromMeters(candidate.TripMeters, unit);

        return new Dictionary<string, object?>
        {
            ["truck_id"] = truck.Id,
            ["truck_number"] = truck.Number,
            ["truck_type"] = truck.Type.ToString(),
            ["trailer_type"] = trailer?.Type.ToString(),
            ["driver_id"] = driver!.Id,
            ["driver_name"] = driver.GetFullName(),
            [$"deadhead_{suffix}"] = candidate.DeadheadMeters is { } m ? DispatchUnits.FromMeters(m, unit) : null,
            [$"total_{suffix}"] = total,
            ["drive_time"] = AgentDuration.Format(candidate.Verdict.EstimatedDrivingMinutes),
            ["driving_time_left"] = hos is null ? null : AgentDuration.Format(hos.DrivingMinutesRemaining),
            ["hos"] = HosLabels[(int)candidate.Hos],
            ["equipment"] = candidate.Equipment.ToString().ToLowerInvariant(),
            ["equipment_note"] = candidate.EquipmentNote,
            [$"revenue_per_{suffix}"] = DispatchUnits.RatePer(load.DeliveryCost, total)
        };
    }

    /// <summary>Declaration order is the ranking order; <see cref="HosLabels"/> follows it.</summary>
    private enum HosFit
    {
        Feasible,
        MultiDay,
        Unknown
    }

    private static readonly string[] HosLabels = ["feasible", "multi_day", "unknown"];

    private sealed record Scored(
        AvailableTruck Entry,
        double? DeadheadMeters,
        double TripMeters,
        HosFeasibility Verdict,
        HosFit Hos,
        EquipmentMatch Equipment,
        string? EquipmentNote);
}
