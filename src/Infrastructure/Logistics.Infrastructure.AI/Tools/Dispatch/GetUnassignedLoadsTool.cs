using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class GetUnassignedLoadsTool(IMediator mediator)
    : AgentTool<NoToolInput>, IAgentToolMetadata
{
    public static AgentToolDefinition Definition => new(
        "get_unassigned_loads",
        "Get all Draft loads that are not assigned to any trip. Returns load ID, name, type, origin, destination, distance, delivery cost, and customer.")
    {
        RequiredPermission = Permission.Dispatch.View,
        DispatchAgent = true
    };

    protected override async Task<string> ExecuteAsync(NoToolInput input, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnassignedLoadsQuery(), ct);

        if (!result.IsSuccess)
            return ToolResult.Error(result.Error);

        var items = result.Value?.ToList() ?? [];
        var loads = items.Select(l => new AgentToolLoadDto
        {
            Id = l.Id,
            Name = l.Name,
            Type = l.Type.ToString(),
            Origin = l.OriginAddress?.ToString(),
            Destination = l.DestinationAddress?.ToString(),
            OriginLat = l.OriginLocation?.Latitude,
            OriginLng = l.OriginLocation?.Longitude,
            DestLat = l.DestinationLocation?.Latitude,
            DestLng = l.DestinationLocation?.Longitude,
            DistanceKm = Math.Round(DispatchUnits.MetersToKm(l.Distance), 1),
            DeliveryCost = l.DeliveryCost,
            Customer = l.Customer?.Name,
            ContainerNumber = l.ContainerNumber,
            ContainerIsoType = l.ContainerIsoType?.ToString(),
            OriginTerminal = FormatTerminal(l.OriginTerminalName, l.OriginTerminalCode),
            DestinationTerminal = FormatTerminal(l.DestinationTerminalName, l.DestinationTerminalCode)
        }).ToList();

        return ToolResult.Typed(new AgentToolResultDto { Loads = loads, Count = items.Count });
    }

    /// <summary>"Los Angeles (USLAX)" - null when the load has no terminal, so it stays out of the JSON.</summary>
    private static string? FormatTerminal(string? name, string? code)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.IsNullOrWhiteSpace(code) ? null : code;

        return string.IsNullOrWhiteSpace(code) ? name : $"{name} ({code})";
    }
}
