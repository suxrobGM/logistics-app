using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Operations.Trips.Commands;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class DispatchTripTool(IMediator mediator) : IAIDispatchTool
{
    public string Name => "dispatch_trip";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (input.GetGuid("trip_id") is not { } tripId)
            return ToolResult.Error("Invalid or missing trip_id");

        var result = await mediator.Send(new DispatchTripCommand { TripId = tripId }, ct);

        return ToolResult.Written(result, new { success = true, trip_id = tripId });
    }
}
