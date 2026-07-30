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
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

internal sealed class AIDispatchService(
    IOptions<LlmOptions> options,
    AIDispatchConversationBuilder conversationBuilder,
    AgentLoopRunner loopRunner,
    AgentSessionCancellationRegistry cancellationRegistry,
    ITenantUnitOfWork tenantUow,
    IAIDispatchBroadcastService broadcastService,
    IStripeUsageService stripeUsageService,
    IAgentRunContext runContext,
    ILogger<AIDispatchService> logger) : IAIDispatchService
{
    public async Task<AIDispatchSession> RunAsync(AIDispatchRequest request, CancellationToken ct = default)
    {
        var blocked = await CheckLlmDisabledAsync(request, ct);
        if (blocked is not null)
            return blocked;

        runContext.SetTriggeredBy(request.TriggeredByUserId);

        var session = new AIDispatchSession
        {
            Mode = request.Mode,
            TriggeredByUserId = request.TriggeredByUserId,
            StartedAt = DateTime.UtcNow,
            IsOverage = request.IsOverage,
            Instructions = request.Instructions
        };

        await tenantUow.Repository<AIDispatchSession>().AddAsync(session, ct);
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
        await ReportOverageIfNeededAsync(session, request.TenantId);
        return session;
    }

    public async Task<bool> CancelAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await tenantUow.Repository<AIDispatchSession>().GetByIdAsync(sessionId, ct);
        if (session is null)
            return false;

        if (session.Status != AIDispatchSessionStatus.Running)
            return false;

        cancellationRegistry.TryCancel(sessionId);
        session.Cancel();
        await tenantUow.SaveChangesAsync(ct);
        return true;
    }

    private async Task RunAgentLoopAsync(
        AIDispatchSession session,
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

    private async Task<AIDispatchSession?> CheckLlmDisabledAsync(
        AIDispatchRequest request, CancellationToken ct)
    {
        if (options.Value.BypassLlmGate)
            return null;

        var tenant = tenantUow.GetCurrentTenant();
        if (tenant.Settings.LlmEnabled != false)
            return null;

        logger.LogInformation("LLM is disabled for tenant {TenantId}, skipping session", request.TenantId);

        var session = new AIDispatchSession
        {
            Mode = request.Mode,
            TriggeredByUserId = request.TriggeredByUserId,
            StartedAt = DateTime.UtcNow
        };
        session.Fail("LLM is disabled for this tenant. Contact your administrator to enable it.");
        await tenantUow.Repository<AIDispatchSession>().AddAsync(session, ct);
        await tenantUow.SaveChangesAsync(ct);
        return session;
    }

    private async Task BroadcastSessionUpdateAsync(AIDispatchSession session)
    {
        try
        {
            var tenantId = tenantUow.GetCurrentTenant().Id;
            await broadcastService.BroadcastSessionUpdateAsync(tenantId, new AIDispatchUpdateDto
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

    private async Task ReportOverageIfNeededAsync(AIDispatchSession session, Guid tenantId)
    {
        if (!session.IsOverage || session.Status != AIDispatchSessionStatus.Completed)
            return;

        try
        {
            var billingUnits = LlmPricing.GetOverageBillingUnits(session.ModelUsed ?? "");
            await stripeUsageService.ReportAISessionOverageAsync(tenantId, billingUnits);
            logger.LogInformation("Reported AI session overage for session {SessionId} ({BillingUnits} units)",
                session.Id, billingUnits);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to report AI session overage for session {SessionId}", session.Id);
        }
    }
}
