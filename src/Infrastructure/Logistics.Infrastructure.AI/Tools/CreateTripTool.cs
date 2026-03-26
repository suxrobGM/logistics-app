using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Commands;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CreateTripTool(IMediator mediator) : IDispatchTool
{
    public string Name => "create_trip";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var truckId = Guid.Parse(input["truck_id"]!.GetValue<string>());
        var loadIds = input["load_ids"]!.AsArray()
            .Select(n => Guid.Parse(n!.GetValue<string>())).ToList();
        var name = input["name"]?.GetValue<string>() ?? "AI-Generated Trip";

        var command = new CreateTripCommand
        {
            Name = name,
            TruckId = truckId,
            AttachedLoadIds = loadIds
        };

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? JsonSerializer.Serialize(new { success = true, trip_id = result.Value })
            : JsonSerializer.Serialize(new { success = false, error = result.Error });
    }
}
