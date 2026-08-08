using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class SendAICopilotMessageHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IAIQuotaService quotaService,
    IBackgroundJobRunner<AICopilotTurnRequest> backgroundRunner,
    ILogger<SendAICopilotMessageHandler> logger)
    : IAppRequestHandler<SendAICopilotMessageCommand, Result<SendAgentMessageResultDto>>
{
    public Task<Result<SendAgentMessageResultDto>> Handle(
        SendAICopilotMessageCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();

        return AgentConversationCommands.SendMessageAsync(
            tenantUow,
            quotaService,
            logger,
            AgentConversationScope.Copilot(userId),
            request.ConversationId,
            request.Text,
            userId,
            (tenantId, conversationId, callerId) =>
                backgroundRunner.Enqueue(new AICopilotTurnRequest(tenantId, conversationId, callerId)),
            ct);
    }
}
