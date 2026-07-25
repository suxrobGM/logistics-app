using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Logistics.Application.Modules.Operations.Loads.Queries;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class GetUnassignedLoadsTool(IMediator mediator) : IAiDispatchTool
{
    public string Name => "get_unassigned_loads";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnassignedLoadsQuery(), ct);

        if (!result.IsSuccess)
            return JsonSerializer.Serialize(new { error = result.Error });

        var items = result.Value?.ToList() ?? [];
        var loads = items.Select(l => new
        {
            id = l.Id,
            name = l.Name,
            type = l.Type.ToString(),
            origin = l.OriginAddress?.ToString(),
            destination = l.DestinationAddress?.ToString(),
            origin_lat = l.OriginLocation?.Latitude,
            origin_lng = l.OriginLocation?.Longitude,
            dest_lat = l.DestinationLocation?.Latitude,
            dest_lng = l.DestinationLocation?.Longitude,
            distance_km = Math.Round(l.Distance / 1000.0, 1),
            delivery_cost = l.DeliveryCost,
            customer = l.Customer?.Name,
            // Intermodal metadata - present only on container loads. Without it the agent has no
            // reason to call get_container_status / get_terminal_info.
            container_number = l.ContainerNumber,
            container_iso_type = l.ContainerIsoType?.ToString(),
            origin_terminal = FormatTerminal(l.OriginTerminalName, l.OriginTerminalCode),
            destination_terminal = FormatTerminal(l.DestinationTerminalName, l.DestinationTerminalCode)
        });

        return JsonSerializer.Serialize(new { loads, count = items.Count });
    }

    /// <summary>"Los Angeles (USLAX)" - null when the load has no terminal, so it stays out of the JSON.</summary>
    private static string? FormatTerminal(string? name, string? code)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.IsNullOrWhiteSpace(code) ? null : code;

        return string.IsNullOrWhiteSpace(code) ? name : $"{name} ({code})";
    }
}
