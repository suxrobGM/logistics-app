using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

/// <summary>
/// Dispatch conversations are tenant-shared: no ownership check, only the endpoint's
/// <c>Permission.Dispatch.Manage</c> policy gates who may send. <c>CreatedById</c> stays whoever
/// created the conversation, for audit only.
/// </summary>
internal sealed class SendAIDispatchMessageHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IAIQuotaService quotaService,
    IBackgroundJobRunner<AIDispatchTurnRequest> backgroundRunner,
    ILogger<SendAIDispatchMessageHandler> logger)
    : IAppRequestHandler<SendAIDispatchMessageCommand, Result<SendAgentMessageResultDto>>
{
    public Task<Result<SendAgentMessageResultDto>> Handle(
        SendAIDispatchMessageCommand request, CancellationToken ct) =>
        AgentConversationCommands.SendMessageAsync(
            tenantUow,
            quotaService,
            logger,
            AgentConversationScope.Dispatch,
            request.ConversationId,
            request.Text,
            currentUser.GetUserId(),
            (tenantId, conversationId, userId) =>
                backgroundRunner.Enqueue(new AIDispatchTurnRequest(tenantId, conversationId, userId)),
            ct);
}
