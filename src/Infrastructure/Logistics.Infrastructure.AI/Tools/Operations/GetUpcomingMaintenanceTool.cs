using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Operations.Maintenance.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Operations;

internal sealed class GetUpcomingMaintenanceTool(IMediator mediator)
    : AgentTool<GetUpcomingMaintenanceTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("Look-ahead window in days (default 30)")]
        public int? DaysAhead { get; init; }

        [Description("Limit to one truck (GUID)")]
        public Guid? TruckId { get; init; }

        [Description("Include already-overdue schedules (default true)")]
        public bool? IncludeOverdue { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "get_upcoming_maintenance",
        "Trucks with maintenance due within the next N days (default 30), including overdue items. Date-based schedules only: mileage and engine-hour intervals are NOT evaluated - say so when answering maintenance questions.")
    {
        RequiredFeature = TenantFeature.Maintenance,
        RequiredPermission = Permission.Maintenance.View
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var query = new GetUpcomingMaintenanceQuery
        {
            DaysAhead = input.DaysAhead ?? 30,
            TruckId = input.TruckId,
            IncludeOverdue = input.IncludeOverdue ?? true
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess || result.Value is null)
            return ToolResult.Error(result.Error ?? "No maintenance data");

        var schedules = result.Value.Select(s => new
        {
            truck_id = s.TruckId,
            truck_number = s.TruckNumber,
            service = s.TypeDisplay,
            description = s.Description,
            next_service_date = s.NextServiceDate?.ToString("yyyy-MM-dd"),
            is_overdue = s.IsOverdue,
            days_until_due = s.DaysUntilDue
        });

        return ToolResult.Ok(new { schedules, count = result.Value.Count });
    }
}
