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
    IBackgroundJobRunner<AICopilotTurnRequest> backgroundRunner,
    ILogger<SendAICopilotMessageHandler> logger)
    : IAppRequestHandler<SendAICopilotMessageCommand, Result<SendAICopilotMessageResultDto>>
{
    /// <summary>
    /// A Running conversation older than this is assumed crashed and may be taken over, rather
    /// than staying locked forever.
    /// </summary>
    private static readonly TimeSpan StaleTurnWindow = TimeSpan.FromMinutes(15);

    public async Task<Result<SendAICopilotMessageResultDto>> Handle(
        SendAICopilotMessageCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        if (userId is null)
            return Result<SendAICopilotMessageResultDto>.Fail("User is not authenticated");

        var conversation = await tenantUow.Repository<AICopilotConversation>()
            .GetByIdAsync(request.ConversationId, ct);

        if (conversation is null || conversation.CreatedById != userId.Value)
            return Result<SendAICopilotMessageResultDto>.Fail("Conversation not found");

        if (conversation.Status == AICopilotConversationStatus.Running)
        {
            if (conversation.TurnStartedAt > DateTime.UtcNow - StaleTurnWindow)
                return Result<SendAICopilotMessageResultDto>.Fail("A copilot turn is already in progress");

            logger.LogWarning(
                "Copilot conversation {ConversationId} stuck Running since {TurnStartedAt}; taking over",
                conversation.Id, conversation.TurnStartedAt);
        }

        // No budget gate here - an over-budget turn runs and is metered as overage by
        // AICopilotService, which owns the session the charge attaches to.
        var tenant = tenantUow.GetCurrentTenant();

        var message = conversation.AddTextMessage(AICopilotMessageRole.User, request.Text.Trim());
        await tenantUow.Repository<AICopilotMessage>().AddAsync(message, ct);
        conversation.BeginTurn();
        await tenantUow.SaveChangesAsync(ct);

        backgroundRunner.Enqueue(new AICopilotTurnRequest(
            tenant.Id, conversation.Id, userId.Value, request.PageContext));

        return Result<SendAICopilotMessageResultDto>.Ok(new SendAICopilotMessageResultDto
        {
            ConversationId = conversation.Id,
            UserMessageId = message.Id
        });
    }
}
