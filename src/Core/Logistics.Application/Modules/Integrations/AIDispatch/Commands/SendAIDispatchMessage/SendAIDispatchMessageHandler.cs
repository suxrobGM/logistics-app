using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

/// <summary>
/// Dispatch conversations are tenant-shared: no ownership check, only the endpoint's
/// <c>Permission.Dispatch.Manage</c> policy gates who may send. <c>CreatedById</c> stays whoever
/// created the conversation, for audit only.
/// </summary>
internal sealed class SendAIDispatchMessageHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser,
    IBackgroundJobRunner<AIDispatchTurnRequest> backgroundRunner,
    IAIDispatchBroadcastService broadcastService)
    : IAppRequestHandler<SendAIDispatchMessageCommand, Result<SendAgentMessageResultDto>>
{
    public Task<Result<SendAgentMessageResultDto>> Handle(
        SendAIDispatchMessageCommand request, CancellationToken ct) =>
        commands.SendMessageAsync(
            AgentConversationScope.Dispatch,
            request.ConversationId,
            request.Text,
            currentUser.GetUserId(),
            (tenantId, conversationId, userId) =>
                backgroundRunner.Enqueue(new AIDispatchTurnRequest(tenantId, conversationId, userId)),
            ct,
            // The board is shared: without this the others see the agent's answer but not the question.
            (tenantId, message) => broadcastService.BroadcastMessageAsync(tenantId, message));
}
