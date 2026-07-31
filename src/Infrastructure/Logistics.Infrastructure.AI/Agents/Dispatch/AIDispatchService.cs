using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

internal sealed class AIDispatchService(
    IOptions<LlmOptions> options,
    AIDispatchConversationBuilder conversationBuilder,
    AgentLoopRunner loopRunner,
    AgentSessionCancellationRegistry cancellationRegistry,
    ITenantUnitOfWork tenantUow,
    IAIDispatchBroadcastService broadcastService,
    IAIQuotaService quotaService,
    AgentOverageReporter overageReporter,
    IAgentRunContext runContext,
    ILogger<AIDispatchService> logger) : IAIDispatchService
{
    public async Task<AgentSession> RunAsync(AIDispatchRequest request, CancellationToken ct = default)
    {
        var blocked = await CheckLlmDisabledAsync(request, ct);
        if (blocked is not null)
            return blocked;

        runContext.SetTriggeredBy(request.TriggeredByUserId);

        // Billed-not-blocked by default: over-quota sessions run and meter on completion, unless
        // the owner opted into a hard pause.
        var quota = await quotaService.GetQuotaStatusAsync(request.TenantId, ct);
        if (quota.OverageBlocked)
        {
            logger.LogInformation(
                "Weekly AI budget reached and overage is blocked for tenant {TenantId}, skipping session",
                request.TenantId);
            return await FailSessionAsync(request, ErrorCodes.AIBudgetReachedMessage, ct);
        }

        var session = new AgentSession
        {
            Mode = request.Mode,
            TriggeredByUserId = request.TriggeredByUserId,
            StartedAt = DateTime.UtcNow,
            IsOverage = quota.IsOverQuota,
            Instructions = request.Instructions
        };

        await tenantUow.Repository<AgentSession>().AddAsync(session, ct);
        await tenantUow.SaveChangesAsync(ct);
        await BroadcastSessionUpdateAsync(session);

        var linkedCt = cancellationRegistry.Register(
            session.Id, ct, TimeSpan.FromMinutes(options.Value.SessionTimeoutMinutes));

        logger.LogInformation(
            "Starting dispatch agent session {SessionId} in {Mode} mode (triggered by {UserId})",
            session.Id, request.Mode, request.TriggeredByUserId?.ToString() ?? "background-job");

        try
        {
            await RunAgentLoopAsync(session, request, linkedCt);
            session.Complete(session.Summary);
            logger.LogInformation(
                "Dispatch agent session {SessionId} completed: {DecisionCount} decisions, {Tokens} tokens",
                session.Id, session.DecisionCount, session.TotalTokensUsed);
        }
        catch (OperationCanceledException)
        {
            session.Cancel();
            logger.LogInformation("Dispatch agent session {SessionId} was cancelled", session.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dispatch agent session {SessionId} failed", session.Id);
            session.Fail(LlmErrorSanitizer.ForSession(ex));
        }
        finally
        {
            cancellationRegistry.Unregister(session.Id);
        }

        await tenantUow.SaveChangesAsync(CancellationToken.None);
        await BroadcastSessionUpdateAsync(session);
        await overageReporter.ReportIfOverBudgetAsync(session, request.TenantId);
        return session;
    }

    public async Task<bool> CancelAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await tenantUow.Repository<AgentSession>().GetByIdAsync(sessionId, ct);
        if (session is null)
            return false;

        if (session.Status != AgentSessionStatus.Running)
            return false;

        cancellationRegistry.TryCancel(sessionId);
        session.Cancel();
        await tenantUow.SaveChangesAsync(ct);
        return true;
    }

    private async Task RunAgentLoopAsync(
        AgentSession session,
        AIDispatchRequest request,
        CancellationToken ct)
    {
        var config = options.Value;
        var conversation = await conversationBuilder.BuildAsync(session, request, config, ct);

        // Broadcast progress after each iteration (decisions already saved + broadcast by the processor)
        await loopRunner.RunAsync(
            session, conversation, new ToolCallContext(request.Mode),
            () => BroadcastSessionUpdateAsync(session), ct);
    }

    private async Task<AgentSession?> CheckLlmDisabledAsync(
        AIDispatchRequest request, CancellationToken ct)
    {
        if (options.Value.BypassAIGate)
            return null;

        var tenant = tenantUow.GetCurrentTenant();
        if (tenant.Settings.AIEnabled != false)
            return null;

        logger.LogInformation("LLM is disabled for tenant {TenantId}, skipping session", request.TenantId);
        return await FailSessionAsync(
            request, "LLM is disabled for this tenant. Contact your administrator to enable it.", ct);
    }

    /// <summary>
    /// Records a refusal as a failed session so the sessions list shows why nothing ran. IsOverage
    /// stays false - a session that never ran must not bill.
    /// </summary>
    private async Task<AgentSession> FailSessionAsync(
        AIDispatchRequest request, string reason, CancellationToken ct)
    {
        var session = new AgentSession
        {
            Mode = request.Mode,
            TriggeredByUserId = request.TriggeredByUserId,
            StartedAt = DateTime.UtcNow
        };
        session.Fail(reason);
        await tenantUow.Repository<AgentSession>().AddAsync(session, ct);
        await tenantUow.SaveChangesAsync(ct);
        return session;
    }

    private async Task BroadcastSessionUpdateAsync(AgentSession session)
    {
        try
        {
            var tenantId = tenantUow.GetCurrentTenant().Id;
            await broadcastService.BroadcastSessionUpdateAsync(tenantId, new AgentSessionUpdateDto
            {
                SessionId = session.Id,
                Status = session.Status,
                Mode = session.Mode,
                DecisionCount = session.DecisionCount,
                Summary = session.Summary
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast dispatch agent update for session {SessionId}", session.Id);
        }
    }

}
