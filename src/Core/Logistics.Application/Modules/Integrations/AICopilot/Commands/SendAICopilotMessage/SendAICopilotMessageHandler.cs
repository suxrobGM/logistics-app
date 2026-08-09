using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class SendAICopilotMessageHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser,
    IBackgroundJobRunner<AICopilotTurnRequest> backgroundRunner)
    : IAppRequestHandler<SendAICopilotMessageCommand, Result<SendAgentMessageResultDto>>
{
    public Task<Result<SendAgentMessageResultDto>> Handle(
        SendAICopilotMessageCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();

        return commands.SendMessageAsync(
            AgentConversationScope.Copilot(userId),
            request.ConversationId,
            request.Text,
            userId,
            (tenantId, conversationId, callerId) =>
                backgroundRunner.Enqueue(new AICopilotTurnRequest(tenantId, conversationId, callerId)),
            ct);
    }
}
