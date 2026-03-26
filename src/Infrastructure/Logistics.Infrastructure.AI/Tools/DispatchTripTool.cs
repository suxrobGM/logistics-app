using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Commands;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class DispatchTripTool(IMediator mediator) : IDispatchTool
{
    public string Name => "dispatch_trip";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        if (!Guid.TryParse(input["trip_id"]?.GetValue<string>(), out var tripId))
            return JsonSerializer.Serialize(new { error = "Invalid or missing trip_id" });

        var result = await mediator.Send(new DispatchTripCommand { TripId = tripId }, ct);

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, trip_id = tripId })
            : JsonSerializer.Serialize(new { success = false, error = result.Error });
    }
}
