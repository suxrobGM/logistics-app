using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
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
    /// <summary>
    /// A Running conversation older than this is assumed crashed and may be taken over, rather
    /// than staying locked forever.
    /// </summary>
    private static readonly TimeSpan StaleTurnWindow = TimeSpan.FromMinutes(15);

    public async Task<Result<SendAgentMessageResultDto>> Handle(
        SendAICopilotMessageCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        if (userId is null)
            return Result<SendAgentMessageResultDto>.Fail("User is not authenticated");

        var conversation = await tenantUow.Repository<AgentConversation>()
            .GetByIdAsync(request.ConversationId, ct);

        if (conversation is null
            || conversation.CreatedById != userId.Value
            || conversation.Kind != AgentConversationKind.Copilot)
        {
            return Result<SendAgentMessageResultDto>.Fail("Conversation not found");
        }

        if (conversation.Status == AgentConversationStatus.Running)
        {
            if (conversation.TurnStartedAt > DateTime.UtcNow - StaleTurnWindow)
                return Result<SendAgentMessageResultDto>.Fail("A copilot turn is already in progress");

            logger.LogWarning(
                "Copilot conversation {ConversationId} stuck Running since {TurnStartedAt}; taking over",
                conversation.Id, conversation.TurnStartedAt);
        }

        var tenant = tenantUow.GetCurrentTenant();

        // Billed-not-blocked by default, so the opt-in flag (already in memory) short-circuits the
        // quota round trips for every tenant that never asked for a hard pause.
        if (tenant.Settings.BlockAIOverage)
        {
            var quota = await quotaService.GetQuotaStatusAsync(tenant.Id, ct);
            if (quota.OverageBlocked)
            {
                return Result<SendAgentMessageResultDto>.Fail(
                    ErrorCodes.AIBudgetReachedMessage, ErrorCodes.AIBudgetReached);
            }
        }

        var message = conversation.AddTextMessage(AgentMessageRole.User, request.Text.Trim());
        await tenantUow.Repository<AgentMessage>().AddAsync(message, ct);
        conversation.BeginTurn();
        await tenantUow.SaveChangesAsync(ct);

        backgroundRunner.Enqueue(new AICopilotTurnRequest(tenant.Id, conversation.Id, userId.Value));

        return Result<SendAgentMessageResultDto>.Ok(new SendAgentMessageResultDto
        {
            ConversationId = conversation.Id,
            UserMessageId = message.Id,
            UserMessageCreatedAt = message.CreatedAt
        });
    }
}
