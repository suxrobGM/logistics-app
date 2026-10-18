using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.AI.Tools.Operations;

internal sealed class GetDetentionTool(IMediator mediator, ITenantUnitOfWork tenantUow)
    : AgentTool<GetDetentionTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Load ID (GUID) for one load's stops. Omit to list every truck waiting at a stop right now")]
        [AgentEntityId(AgentEntityKind.Load)]
        public Guid? LoadId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_detention",
        "Dwell time and detention at stops. With load_id: every stop of that load the truck has reached - dwell, free time, minutes over, detention charge, and whether it is already billed on the load. Without load_id: every truck at a stop it has not departed yet, most overdue first. Times are in minutes. Detention is billed automatically when the truck departs.")
    {
        RequiredFeature = TenantFeature.Loads,
        RequiredPermission = Permission.Load.View,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Mcp
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct) =>
        input.LoadId is { } loadId ? await ForLoadAsync(loadId, ct) : await WaitingNowAsync(ct);

    private async Task<string> ForLoadAsync(Guid loadId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLoadDetentionQuery { LoadId = loadId }, ct);
        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "Load not found");

        var stops = result.Value.Select(s => new
        {
            stop_type = s.StopType.ToString(),
            arrived_at = s.ArrivedAt.ToString("yyyy-MM-dd HH:mm"),
            departed_at = s.DepartedAt?.ToString("yyyy-MM-dd HH:mm"),
            dwell_minutes = s.DwellMinutes,
            free_minutes = s.FreeMinutes,
            over_minutes = s.OverMinutes,
            rate_per_hour = s.RatePerHour,
            charge = s.Charge,
            is_ongoing = s.IsOngoing,
            is_billed = s.IsBilled
        }).ToList();

        return ToolResult.Ok(new { stops, count = stops.Count });
    }

    private async Task<string> WaitingNowAsync(CancellationToken ct)
    {
        var waiting = await tenantUow.Repository<TripStop>().Query()
            .Where(s => s.ArrivedAt != null && s.DepartedAt == null)
            .Select(s => new
            {
                Stop = s,
                LoadId = s.Load.Id,
                LoadNumber = s.Load.Number,
                s.Load.Customer,
                TruckNumber = s.Trip.Truck != null ? s.Trip.Truck.Number : null
            })
            .ToListAsync(ct);

        var tenantSettings = tenantUow.GetCurrentTenant().Settings;
        var now = DateTime.UtcNow;

        var stops = waiting
            .Select(row => (Row: row, Detention: DetentionCalculator.Calculate(
                row.Stop, now, DetentionTerms.For(tenantSettings, row.Customer))!))
            .OrderByDescending(x => x.Detention.OverMinutes)
            .ThenByDescending(x => x.Detention.DwellMinutes)
            .Take(ToolResult.MaxResults)
            .Select(x => new
            {
                load_id = x.Row.LoadId,
                load_number = x.Row.LoadNumber,
                truck_number = x.Row.TruckNumber,
                customer = x.Row.Customer?.Name,
                stop_type = x.Row.Stop.Type.ToString(),
                arrived_at = x.Row.Stop.ArrivedAt!.Value.ToString("yyyy-MM-dd HH:mm"),
                dwell_minutes = x.Detention.DwellMinutes,
                free_minutes = x.Detention.FreeMinutes,
                over_minutes = x.Detention.OverMinutes,
                accrued_charge = x.Detention.Charge
            })
            .ToList();

        return ToolResult.Paged("waiting", stops, waiting.Count);
    }
}
