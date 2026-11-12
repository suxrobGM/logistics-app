using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.Operations;

internal sealed class SendCustomerUpdateTool(IMediator mediator)
    : AgentTool<SendCustomerUpdateTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The load (GUID) whose customer should hear about the delay")]
        [AgentEntityId(AgentEntityKind.Load)]
        public required Guid LoadId { get; init; }

        [Description("Two or three plain sentences to the customer: what changed, the new expected arrival, " +
                     "and what the carrier is doing about it. No internal details, driver names or hours.")]
        public required string Message { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        AgentToolNames.SendCustomerUpdate,
        "Email the load's customer a short delay notice in the carrier's running-late template, with the " +
        "tracking link. Use it when a trip alert shows the delivery will miss its appointment window.")
    {
        RequiredFeature = TenantFeature.CustomerUpdates,
        RequiredPermission = Permission.Load.Manage,
        DecisionType = AgentDecisionType.SendCustomerUpdate,
        Surfaces = AgentSurfaces.Copilot | AgentSurfaces.Dispatch
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Message))
        {
            return ToolResult.Error("message must not be empty");
        }

        var result = await mediator.Send(
            new SendCustomerDelayNoticeCommand { LoadId = input.LoadId, Message = input.Message }, ct);

        return ToolResult.Written(result, new { success = true, load_id = input.LoadId });
    }
}
