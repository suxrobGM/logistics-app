using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class CancelAICopilotTurnHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser) : IAppRequestHandler<CancelAICopilotTurnCommand, Result>
{
    public Task<Result> Handle(CancelAICopilotTurnCommand request, CancellationToken ct) =>
        commands.CancelTurnAsync(
            AgentConversationScope.Copilot(currentUser.GetUserId()), request.ConversationId, ct);
}
