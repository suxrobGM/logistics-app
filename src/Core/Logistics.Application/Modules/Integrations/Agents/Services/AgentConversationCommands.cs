using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal sealed class AgentConversationCommands(
    ITenantUnitOfWork tenantUow,
    IAgentConversationAccess access,
    IAIQuotaService quotaService,
    IAIDispatchService dispatchService,
    ILogger<AgentConversationCommands> logger) : IAgentConversationCommands
{
    /// <summary>
    /// A Running conversation older than this is assumed crashed and may be taken over, rather
    /// than staying locked forever.
    /// </summary>
    private static readonly TimeSpan StaleTurnWindow = TimeSpan.FromMinutes(15);

    public async Task<Result<AgentConversationDto>> CreateAsync(
        AgentConversationKind kind, Guid? userId, CancellationToken ct)
    {
        if (userId is null)
            return Result<AgentConversationDto>.Fail("User is not authenticated");

        var conversation = new AgentConversation { CreatedById = userId.Value, Kind = kind };
        await tenantUow.Repository<AgentConversation>().AddAsync(conversation, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<AgentConversationDto>.Ok(conversation.ToDto());
    }

    public async Task<Result> RenameAsync(
        AgentConversationScope scope, Guid conversationId, string title, CancellationToken ct)
    {
        var conversation = await access.LoadAsync(conversationId, scope, ct);
        if (conversation is null)
            return Result.Fail("Conversation not found");

        conversation.Title = title.Trim();
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(
        AgentConversationScope scope, Guid conversationId, CancellationToken ct)
    {
        var conversation = await access.LoadAsync(conversationId, scope, ct);
        if (conversation is null)
            return Result.Fail("Conversation not found");

        if (conversation.Status == AgentConversationStatus.Running)
            return Result.Fail("Cannot delete a conversation while a turn is running");

        // Cascades to messages, turn sessions, and their decisions.
        tenantUow.Repository<AgentConversation>().Delete(conversation);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> CancelTurnAsync(
        AgentConversationScope scope, Guid conversationId, CancellationToken ct)
    {
        var conversation = await access.LoadAsync(conversationId, scope, ct);
        if (conversation is null)
            return Result.Fail("Conversation not found");

        var runningSessionId = await tenantUow.Repository<AgentSession>().Query()
            .Where(s => s.ConversationId == conversation.Id && s.Status == AgentSessionStatus.Running)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);

        // Cancellation is cooperative - the turn's own finally block calls EndTurn. Only a turn
        // with no live session needs unsticking here.
        if (runningSessionId is { } sessionId)
        {
            await dispatchService.CancelAsync(sessionId, ct);
            return Result.Ok();
        }

        conversation.EndTurn();
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<SendAgentMessageResultDto>> SendMessageAsync(
        AgentConversationScope scope,
        Guid conversationId,
        string text,
        Guid? userId,
        Action<Guid, Guid, Guid> enqueueTurn,
        CancellationToken ct)
    {
        if (userId is null)
            return Result<SendAgentMessageResultDto>.Fail("User is not authenticated");

        var conversation = await access.LoadAsync(conversationId, scope, ct);
        if (conversation is null)
            return Result<SendAgentMessageResultDto>.Fail("Conversation not found");

        if (conversation.Status == AgentConversationStatus.Running)
        {
            if (conversation.TurnStartedAt > DateTime.UtcNow - StaleTurnWindow)
            {
                return Result<SendAgentMessageResultDto>.Fail(
                    $"A {scope.Kind.ToString().ToLowerInvariant()} turn is already in progress");
            }

            logger.LogWarning(
                "{Kind} conversation {ConversationId} stuck Running since {TurnStartedAt}; taking over",
                scope.Kind, conversation.Id, conversation.TurnStartedAt);
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

        var message = conversation.AddTextMessage(AgentMessageRole.User, text.Trim());
        await tenantUow.Repository<AgentMessage>().AddAsync(message, ct);
        conversation.BeginTurn();
        await tenantUow.SaveChangesAsync(ct);

        enqueueTurn(tenant.Id, conversation.Id, userId.Value);

        return Result<SendAgentMessageResultDto>.Ok(new SendAgentMessageResultDto
        {
            ConversationId = conversation.Id,
            UserMessageId = message.Id,
            UserMessageCreatedAt = message.CreatedAt,
            UserMessageSequence = message.Sequence
        });
    }
}
