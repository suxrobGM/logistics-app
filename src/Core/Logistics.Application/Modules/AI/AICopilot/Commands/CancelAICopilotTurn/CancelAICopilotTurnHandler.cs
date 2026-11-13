using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.AI.Agents;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AICopilot.Commands;

internal sealed class CancelAICopilotTurnHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser) : IRequestHandler<CancelAICopilotTurnCommand, Result>
{
    public Task<Result> Handle(CancelAICopilotTurnCommand request, CancellationToken ct) =>
        commands.CancelTurnAsync(
            AgentConversationScope.Copilot(currentUser.GetUserId()), request.ConversationId, ct);
}
