using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Operations.Maintenance.Queries;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetUpcomingMaintenanceTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "get_upcoming_maintenance";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var query = new GetUpcomingMaintenanceQuery
        {
            DaysAhead = input.GetInt("days_ahead") ?? 30,
            TruckId = input.GetGuid("truck_id"),
            IncludeOverdue = input.GetBool("include_overdue") ?? true
        };

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess || result.Value is null)
            return JsonSerializer.Serialize(new { error = result.Error ?? "No maintenance data" });

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

        return JsonSerializer.Serialize(new { schedules, count = result.Value.Count });
    }
}
