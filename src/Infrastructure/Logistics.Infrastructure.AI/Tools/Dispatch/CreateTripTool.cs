using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using MediatR;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

internal sealed class CreateTripTool(IMediator mediator)
    : AgentTool<CreateTripTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The truck ID (GUID) for this trip")]
        [AgentEntityId(AgentEntityKind.Truck)]
        public required Guid TruckId { get; init; }

        [Description("List of load IDs (GUIDs) to include in the trip")]
        public required Guid[] LoadIds { get; init; }

        [Description("A descriptive name for the trip")]
        public required string Name { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "create_trip",
        "Create a new trip from a set of loads assigned to a truck. Groups multiple loads into an optimized multi-stop trip.")
    {
        RequiredPermission = Permission.Dispatch.Manage,
        DecisionType = AgentDecisionType.CreateTrip,
        Surfaces = AgentSurfaces.All
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (input.LoadIds.Length == 0)
            return ToolResult.Error("Missing or empty load_ids");

        var command = new CreateTripCommand
        {
            Name = input.Name,
            TruckId = input.TruckId,
            AttachedLoadIds = input.LoadIds
        };

        var result = await mediator.Send(command, ct);

        return ToolResult.Written(result, new { success = true, trip_id = result.Value });
    }
}
