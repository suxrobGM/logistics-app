using System.ComponentModel;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class DispatchTripTool(IMediator mediator)
    : AgentTool<DispatchTripTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The trip ID (GUID) to dispatch")]
        [AgentEntityId(AgentEntityKind.Trip)]
        public required Guid TripId { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "dispatch_trip",
        "Dispatch a trip, transitioning it from Draft to Dispatched status. This notifies the driver and starts the trip.")
    {
        RequiredPermission = Permission.AIDispatch.Manage,
        DecisionType = AgentDecisionType.DispatchTrip,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Dispatch
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var result = await mediator.Send(new DispatchTripCommand { TripId = input.TripId }, ct);

        return ToolResult.Written(result, new { success = true, trip_id = input.TripId });
    }
}
