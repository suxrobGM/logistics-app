using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Owns the full agent-turn lifecycle: AI-enabled and quota gates, session creation, cancellation,
/// running the agent loop, persisting the transcript, and broadcasting - parameterized by
/// <see cref="IAgentSurface"/> so every conversational agent (copilot today, dispatch from Phase 3)
/// runs through exactly one turn implementation.
/// </summary>
internal sealed class AgentTurnService(
    IOptions<LlmOptions> options,
    AgentLoopRunner loopRunner,
    AgentSessionCancellationRegistry cancellationRegistry,
    ITenantUnitOfWork tenantUow,
    IAIQuotaService quotaService,
    AgentOverageReporter overageReporter,
    IAgentRunContext runContext,
    ILogger<AgentTurnService> logger)
{
    public async Task RunTurnAsync(AgentTurnRequest request, IAgentSurface surface, CancellationToken ct = default)
    {
        var conversation = await tenantUow.Repository<AgentConversation>()
            .GetByIdAsync(request.ConversationId, ct);

        if (conversation is null)
        {
            logger.LogWarning("Agent conversation {ConversationId} not found", request.ConversationId);
            return;
        }

        if (!options.Value.BypassAIGate && tenantUow.GetCurrentTenant().Settings.AIEnabled == false)
        {
            logger.LogInformation(
                "LLM is disabled for tenant {TenantId}, skipping {SessionType} turn",
                request.TenantId, surface.SessionType);
            await AppendSystemNoticeAsync(
                request, surface, conversation,
                "AI is disabled for this company. Contact your administrator to enable it.", ct);
            return;
        }

        // Billed-not-blocked by default. The send handler already gates; this catches turns
        // enqueued just before the budget line or the toggle flip.
        var quota = await quotaService.GetQuotaStatusAsync(request.TenantId, ct);
        if (quota.OverageBlocked)
        {
            logger.LogInformation(
                "Weekly AI budget reached and overage is blocked for tenant {TenantId}, skipping {SessionType} turn",
                request.TenantId, surface.SessionType);
            await AppendSystemNoticeAsync(request, surface, conversation, ErrorCodes.AIBudgetReachedMessage, ct);
            return;
        }

        runContext.SetTriggeredBy(request.TriggeredByUserId);

        var session = new AgentSession
        {
            Type = surface.SessionType,
            ConversationId = conversation.Id,
            TriggeredByUserId = request.TriggeredByUserId,
            StartedAt = DateTime.UtcNow,
            IsOverage = quota.IsOverQuota && quota.OverageBillable
        };

        await tenantUow.Repository<AgentSession>().AddAsync(session, ct);
        await tenantUow.SaveChangesAsync(ct);

        var linkedCt = cancellationRegistry.Register(
            session.Id, ct, TimeSpan.FromMinutes(options.Value.SessionTimeoutMinutes));
        await BroadcastTurnUpdateSafeAsync(request, surface, conversation, session);

        logger.LogInformation(
            "Starting {SessionType} turn {SessionId} for conversation {ConversationId} (user {UserId})",
            surface.SessionType, session.Id, conversation.Id, request.TriggeredByUserId);

        LlmConversation? state = null;
        var priorMessageCount = 0;

        try
        {
            var setup = await surface.PrepareAsync(session, conversation, request, options.Value, linkedCt);
            state = setup.Conversation;
            priorMessageCount = state.Messages.Count;

            await loopRunner.RunAsync(
                session, state, setup.ToolContext,
                () => BroadcastTurnUpdateSafeAsync(request, surface, conversation, session), linkedCt);

            session.Complete(session.Summary);
        }
        catch (OperationCanceledException)
        {
            session.Cancel();
            logger.LogInformation("{SessionType} turn {SessionId} was cancelled", surface.SessionType, session.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{SessionType} turn {SessionId} failed", surface.SessionType, session.Id);
            session.Fail(LlmErrorSanitizer.ForSession(ex));
        }
        finally
        {
            cancellationRegistry.Unregister(session.Id);
        }

        // Persist even on failure - the audit trail and the next turn's context both depend on it.
        List<AgentMessage> newMessages = state is null
            ? []
            : await PersistTurnMessagesAsync(conversation, session, state.Messages.Skip(priorMessageCount));

        conversation.Title ??= DeriveTitle(conversation);
        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.EndTurn();
        await tenantUow.SaveChangesAsync(CancellationToken.None);

        foreach (var message in newMessages.Where(m => m.DisplayText is not null))
        {
            await BroadcastMessageSafeAsync(request, surface, conversation, message);
        }

        await BroadcastTurnUpdateSafeAsync(request, surface, conversation, session);
        await overageReporter.ReportIfOverBudgetAsync(session, request.TenantId);
    }

    /// <summary>
    /// Maps the turn's appended LLM messages onto transcript rows. Tool-result rows get a null
    /// DisplayText - replayed to the provider, never rendered.
    /// </summary>
    private async Task<List<AgentMessage>> PersistTurnMessagesAsync(
        AgentConversation conversation,
        AgentSession session,
        IEnumerable<LlmMessage> appended)
    {
        var nextSequence = conversation.NextSequence();

        // The user message that triggered this turn was created before the session existed.
        var triggerMessage = conversation.Messages
            .Where(m => m.Role == AgentMessageRole.User && m.SessionId is null)
            .OrderByDescending(m => m.Sequence)
            .FirstOrDefault();
        if (triggerMessage is not null)
            triggerMessage.SessionId = session.Id;

        var rows = new List<AgentMessage>();
        foreach (var message in appended)
        {
            var textParts = message.Content.OfType<LlmTextBlock>().Select(t => t.Text).ToList();
            var displayText = textParts.Count > 0 ? string.Join("\n\n", textParts) : null;

            var row = new AgentMessage
            {
                ConversationId = conversation.Id,
                Sequence = nextSequence++,
                Role = message.Role == LlmRole.Assistant
                    ? AgentMessageRole.Assistant
                    : AgentMessageRole.User,
                ContentJson = AgentTranscriptCodec.Encode(message.Content),
                DisplayText = displayText?.Length > 4000 ? displayText[..4000] : displayText,
                SessionId = session.Id
            };

            conversation.Messages.Add(row);
            await tenantUow.Repository<AgentMessage>().AddAsync(row, CancellationToken.None);
            rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// Refusal path: ends the turn with a transcript notice and no session, so nothing bills.
    /// </summary>
    private async Task AppendSystemNoticeAsync(
        AgentTurnRequest request, IAgentSurface surface, AgentConversation conversation, string notice,
        CancellationToken ct)
    {
        var row = conversation.AddTextMessage(AgentMessageRole.System, notice);
        await tenantUow.Repository<AgentMessage>().AddAsync(row, ct);
        conversation.EndTurn();
        await tenantUow.SaveChangesAsync(ct);
        await BroadcastMessageSafeAsync(request, surface, conversation, row);
    }

    private static string? DeriveTitle(AgentConversation conversation)
    {
        var firstUserText = conversation.Messages
            .Where(m => m.Role == AgentMessageRole.User && m.DisplayText is not null)
            .OrderBy(m => m.Sequence)
            .Select(m => m.DisplayText!)
            .FirstOrDefault();

        if (firstUserText is null)
            return null;

        var singleLine = firstUserText.ReplaceLineEndings(" ").Trim();
        return singleLine.Length > 120 ? singleLine[..120] : singleLine;
    }

    private async Task BroadcastMessageSafeAsync(
        AgentTurnRequest request, IAgentSurface surface, AgentConversation conversation, AgentMessage message)
    {
        try
        {
            await surface.BroadcastMessageAsync(request, conversation, message.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast {SessionType} message {MessageId}", surface.SessionType, message.Id);
        }
    }

    private async Task BroadcastTurnUpdateSafeAsync(
        AgentTurnRequest request, IAgentSurface surface, AgentConversation conversation, AgentSession session)
    {
        try
        {
            await surface.BroadcastTurnUpdateAsync(request, conversation, session);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast {SessionType} turn update for session {SessionId}", surface.SessionType, session.Id);
        }
    }
}
