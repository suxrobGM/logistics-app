using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
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
    : IAppRequestHandler<SendAIDispatchMessageCommand, Result<SendAIDispatchMessageResultDto>>
{
    /// <summary>
    /// A Running conversation older than this is assumed crashed and may be taken over, rather
    /// than staying locked forever.
    /// </summary>
    private static readonly TimeSpan StaleTurnWindow = TimeSpan.FromMinutes(15);

    public async Task<Result<SendAIDispatchMessageResultDto>> Handle(
        SendAIDispatchMessageCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        if (userId is null)
            return Result<SendAIDispatchMessageResultDto>.Fail("User is not authenticated");

        var conversation = await tenantUow.Repository<AgentConversation>()
            .GetByIdAsync(request.ConversationId, ct);

        if (conversation is null || conversation.Kind != AgentConversationKind.Dispatch)
            return Result<SendAIDispatchMessageResultDto>.Fail("Conversation not found");

        if (conversation.Status == AICopilotConversationStatus.Running)
        {
            if (conversation.TurnStartedAt > DateTime.UtcNow - StaleTurnWindow)
                return Result<SendAIDispatchMessageResultDto>.Fail("A dispatch turn is already in progress");

            logger.LogWarning(
                "Dispatch conversation {ConversationId} stuck Running since {TurnStartedAt}; taking over",
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
                return Result<SendAIDispatchMessageResultDto>.Fail(
                    ErrorCodes.AIBudgetReachedMessage, ErrorCodes.AIBudgetReached);
            }
        }

        var message = conversation.AddTextMessage(AgentMessageRole.User, request.Text.Trim());
        await tenantUow.Repository<AgentMessage>().AddAsync(message, ct);
        conversation.BeginTurn();
        await tenantUow.SaveChangesAsync(ct);

        backgroundRunner.Enqueue(new AIDispatchTurnRequest(tenant.Id, conversation.Id, userId.Value));

        return Result<SendAIDispatchMessageResultDto>.Ok(new SendAIDispatchMessageResultDto
        {
            ConversationId = conversation.Id,
            UserMessageId = message.Id,
            UserMessageCreatedAt = message.CreatedAt
        });
    }
}
