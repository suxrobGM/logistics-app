using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.Agents;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

/// <summary>Saves the user's message to a dispatch conversation and queues the agent turn as a background job. Dispatch conversations are shared by the whole tenant, so there is no per-user ownership check.</summary>
internal sealed class SendAIDispatchMessageHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser,
    IBackgroundJobRunner<AIDispatchTurnRequest> backgroundRunner,
    IAIDispatchBroadcastService broadcastService)
    : IRequestHandler<SendAIDispatchMessageCommand, Result<SendAgentMessageResultDto>>
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
