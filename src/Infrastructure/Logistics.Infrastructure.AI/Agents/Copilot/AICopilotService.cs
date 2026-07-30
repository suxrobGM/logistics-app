using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Modules.IdentityAccess.Users.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Mappings;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Agents.Copilot;

/// <summary>
/// Runs one conversational copilot turn as an <see cref="AIDispatchSession"/> of type Copilot, so
/// quota, tokens, and decisions ride the existing session machinery.
/// </summary>
internal sealed class AICopilotService(
    IOptions<LlmOptions> options,
    AICopilotConversationBuilder conversationBuilder,
    AgentLoopRunner loopRunner,
    AgentSessionCancellationRegistry cancellationRegistry,
    ITenantUnitOfWork tenantUow,
    IAICopilotBroadcastService broadcastService,
    IAgentRunContext runContext,
    IMediator mediator,
    ILogger<AICopilotService> logger) : IAICopilotService
{
    public async Task RunTurnAsync(AICopilotTurnRequest request, CancellationToken ct = default)
    {
        var conversation = await tenantUow.Repository<AICopilotConversation>()
            .GetByIdAsync(request.ConversationId, ct);

        if (conversation is null)
        {
            logger.LogWarning("Copilot conversation {ConversationId} not found", request.ConversationId);
            return;
        }

        if (await AppendLlmDisabledNoticeAsync(request, conversation, ct))
            return;

        runContext.SetTriggeredBy(request.UserId);
        var permissions = await ResolveCallerPermissionsAsync(request, ct);

        var session = new AIDispatchSession
        {
            Type = AIDispatchSessionType.Copilot,
            ConversationId = conversation.Id,
            Mode = AIDispatchMode.HumanInTheLoop,
            TriggeredByUserId = request.UserId,
            StartedAt = DateTime.UtcNow
        };

        await tenantUow.Repository<AIDispatchSession>().AddAsync(session, ct);
        await tenantUow.SaveChangesAsync(ct);

        var linkedCt = cancellationRegistry.Register(
            session.Id, ct, TimeSpan.FromMinutes(options.Value.SessionTimeoutMinutes));
        await BroadcastTurnUpdateAsync(request, conversation, session);

        logger.LogInformation(
            "Starting copilot turn {SessionId} for conversation {ConversationId} (user {UserId})",
            session.Id, conversation.Id, request.UserId);

        LlmConversation? state = null;
        var priorMessageCount = 0;

        try
        {
            state = await conversationBuilder.BuildAsync(session, conversation, permissions, options.Value, linkedCt);
            priorMessageCount = state.Messages.Count;

            var toolContext = new ToolCallContext(
                AIDispatchMode.HumanInTheLoop,
                CallerPermissions: permissions,
                DecisionBroadcastOverride: dto =>
                    broadcastService.BroadcastDecisionAsync(request.TenantId, conversation.CreatedById, dto));

            await loopRunner.RunAsync(
                session, state, toolContext,
                () => BroadcastTurnUpdateAsync(request, conversation, session), linkedCt);

            session.Complete(session.Summary);
        }
        catch (OperationCanceledException)
        {
            session.Cancel();
            logger.LogInformation("Copilot turn {SessionId} was cancelled", session.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Copilot turn {SessionId} failed", session.Id);
            session.Fail(LlmErrorSanitizer.ForSession(ex));
        }
        finally
        {
            cancellationRegistry.Unregister(session.Id);
        }

        // Persist even on failure - the audit trail and the next turn's context both depend on it.
        var newMessages = state is null
            ? []
            : PersistTurnMessages(conversation, session, state.Messages.Skip(priorMessageCount));

        conversation.Title ??= DeriveTitle(conversation);
        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.EndTurn();
        await tenantUow.SaveChangesAsync(CancellationToken.None);

        foreach (var message in newMessages.Where(m => m.DisplayText is not null))
        {
            await BroadcastMessageAsync(request, conversation, message);
        }

        await BroadcastTurnUpdateAsync(request, conversation, session);
    }

    /// <summary>
    /// Maps the turn's appended LLM messages onto transcript rows. Tool-result rows get a null
    /// DisplayText - replayed to the provider, never rendered.
    /// </summary>
    private List<AICopilotMessage> PersistTurnMessages(
        AICopilotConversation conversation,
        AIDispatchSession session,
        IEnumerable<LlmMessage> appended)
    {
        var nextSequence = conversation.NextSequence();

        // The user message that triggered this turn was created before the session existed.
        var triggerMessage = conversation.Messages
            .Where(m => m.Role == AICopilotMessageRole.User && m.SessionId is null)
            .OrderByDescending(m => m.Sequence)
            .FirstOrDefault();
        if (triggerMessage is not null)
            triggerMessage.SessionId = session.Id;

        var rows = new List<AICopilotMessage>();
        foreach (var message in appended)
        {
            var textParts = message.Content.OfType<LlmTextBlock>().Select(t => t.Text).ToList();
            var displayText = textParts.Count > 0 ? string.Join("\n\n", textParts) : null;

            var row = new AICopilotMessage
            {
                ConversationId = conversation.Id,
                Sequence = nextSequence++,
                Role = message.Role == LlmRole.Assistant
                    ? AICopilotMessageRole.Assistant
                    : AICopilotMessageRole.User,
                ContentJson = CopilotTranscriptCodec.Encode(message.Content),
                DisplayText = displayText?.Length > 4000 ? displayText[..4000] : displayText,
                SessionId = session.Id
            };

            conversation.Messages.Add(row);
            rows.Add(row);
        }

        return rows;
    }

    private async Task<IReadOnlySet<string>> ResolveCallerPermissionsAsync(
        AICopilotTurnRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCurrentUserPermissionsQuery
        {
            UserId = request.UserId,
            TenantId = request.TenantId
        }, ct);

        return result.Value?.ToHashSet() ?? [];
    }

    /// <summary>Returns true when the tenant's LLM access is switched off and the turn must not run.</summary>
    private async Task<bool> AppendLlmDisabledNoticeAsync(
        AICopilotTurnRequest request, AICopilotConversation conversation, CancellationToken ct)
    {
        if (options.Value.BypassLlmGate || tenantUow.GetCurrentTenant().Settings.LlmEnabled != false)
            return false;

        logger.LogInformation("LLM is disabled for tenant {TenantId}, skipping copilot turn", request.TenantId);

        const string notice = "AI is disabled for this company. Contact your administrator to enable it.";
        var row = conversation.AddTextMessage(AICopilotMessageRole.System, notice);
        conversation.EndTurn();
        await tenantUow.SaveChangesAsync(ct);
        await BroadcastMessageAsync(request, conversation, row);
        return true;
    }

    private static string? DeriveTitle(AICopilotConversation conversation)
    {
        var firstUserText = conversation.Messages
            .Where(m => m.Role == AICopilotMessageRole.User && m.DisplayText is not null)
            .OrderBy(m => m.Sequence)
            .Select(m => m.DisplayText!)
            .FirstOrDefault();

        if (firstUserText is null)
            return null;

        var singleLine = firstUserText.ReplaceLineEndings(" ").Trim();
        return singleLine.Length > 120 ? singleLine[..120] : singleLine;
    }

    private async Task BroadcastMessageAsync(
        AICopilotTurnRequest request, AICopilotConversation conversation, AICopilotMessage message)
    {
        try
        {
            await broadcastService.BroadcastMessageAsync(
                request.TenantId, conversation.CreatedById, message.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast copilot message {MessageId}", message.Id);
        }
    }

    private async Task BroadcastTurnUpdateAsync(
        AICopilotTurnRequest request, AICopilotConversation conversation, AIDispatchSession session)
    {
        try
        {
            await broadcastService.BroadcastTurnUpdateAsync(request.TenantId, conversation.CreatedById,
                new AICopilotTurnUpdateDto
                {
                    ConversationId = conversation.Id,
                    SessionId = session.Id,
                    Status = session.Status,
                    TotalTokensUsed = session.TotalTokensUsed,
                    DecisionCount = session.DecisionCount,
                    ErrorMessage = session.ErrorMessage
                });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast copilot turn update for session {SessionId}", session.Id);
        }
    }
}
