using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class DispatchAgentService(
    IOptions<LlmOptions> options,
    DispatchConversationBuilder conversationBuilder,
    DispatchDecisionProcessor decisionProcessor,
    DispatchSessionCancellationRegistry cancellationRegistry,
    ITenantUnitOfWork tenantUow,
    IDispatchAgentBroadcastService broadcastService,
    IStripeUsageService stripeUsageService,
    ILogger<DispatchAgentService> logger) : IDispatchAgentService
{
    private const int MaxIterations = 25;
    private const int MaxRetries = 3;
    private static readonly int[] RetryDelaysMs = [2000, 4000, 8000];

    public async Task<DispatchSession> RunAsync(DispatchAgentRequest request, CancellationToken ct = default)
    {
        var blocked = await CheckLlmDisabledAsync(request, ct);
        if (blocked is not null)
            return blocked;

        var session = new DispatchSession
        {
            Mode = request.Mode,
            TriggeredByUserId = request.TriggeredByUserId,
            StartedAt = DateTime.UtcNow,
            IsOverage = request.IsOverage,
            Instructions = request.Instructions
        };

        await tenantUow.Repository<DispatchSession>().AddAsync(session, ct);
        await tenantUow.SaveChangesAsync(ct);
        await BroadcastSessionUpdateAsync(session);

        var linkedCt = cancellationRegistry.Register(session.Id, ct);

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
            session.Fail(SanitizeErrorMessage(ex));
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
        var session = await tenantUow.Repository<DispatchSession>().GetByIdAsync(sessionId, ct);
        if (session is null)
            return false;

        if (session.Status != DispatchSessionStatus.Running)
            return false;

        cancellationRegistry.TryCancel(sessionId);
        session.Cancel();
        await tenantUow.SaveChangesAsync(ct);
        return true;
    }

    private async Task RunAgentLoopAsync(
        DispatchSession session,
        DispatchAgentRequest request,
        CancellationToken ct)
    {
        var config = options.Value;
        var conversation = await conversationBuilder.BuildAsync(session, request, config);

        var totalInputTokens = 0;
        var totalOutputTokens = 0;
        var totalCacheReadTokens = 0;
        var totalCacheCreationTokens = 0;

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();

            var llmRequest = new LlmRequest
            {
                SystemPrompt = conversation.SystemPrompt,
                Messages = conversation.Messages,
                Tools = [.. conversation.Tools],
                Model = conversation.Model,
                MaxTokens = conversation.MaxTokens,
                Temperature = conversation.Thinking is not null ? null : 0m,
                Thinking = conversation.Thinking
            };

            var result = await SendWithRetryAsync(conversation.Provider, llmRequest, session, ct);

            // Update token usage
            totalInputTokens += result.Usage.InputTokens;
            totalOutputTokens += result.Usage.OutputTokens;
            totalCacheReadTokens += result.Usage.CacheReadTokens;
            totalCacheCreationTokens += result.Usage.CacheCreationTokens;

            session.InputTokensUsed = totalInputTokens;
            session.OutputTokensUsed = totalOutputTokens;
            session.CacheReadTokens = totalCacheReadTokens;
            session.CacheCreationTokens = totalCacheCreationTokens;

            conversation.Messages.Add(result.AssistantMessage);

            if (result.TextContent is not null)
                session.Summary = result.TextContent;

            if (result.StopReason == "end_turn" || result.ToolCalls.Count == 0)
            {
                logger.LogInformation(
                    "Agent session {SessionId} completed after {Iterations} iterations, {Tokens} tokens",
                    session.Id, iteration + 1, session.TotalTokensUsed);
                break;
            }

            var toolResults = await decisionProcessor.ProcessToolCallsAsync(
                session, request.Mode, result.ToolCalls, result.TextContent, ct);

            conversation.Messages.Add(LlmMessage.FromToolResults(toolResults));

            // Broadcast session progress after each iteration (decisions already saved + broadcast by ProcessToolCallsAsync)
            await BroadcastSessionUpdateAsync(session);
        }

        // Set quota cost and estimated cost
        var modelUsed = session.ModelUsed ?? config.GetProviderConfig(config.DefaultProvider).Model;
        session.RequestCost = LlmPricing.GetMultiplier(modelUsed);
        session.EstimatedCostUsd = LlmPricing.Calculate(
            session.ModelUsed ?? config.GetProviderConfig(config.DefaultProvider).Model,
            totalInputTokens, totalOutputTokens,
            totalCacheReadTokens, totalCacheCreationTokens);
    }

    private async Task<LlmResponse> SendWithRetryAsync(
        ILlmProvider provider,
        LlmRequest request,
        DispatchSession session,
        CancellationToken ct)
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await provider.SendAsync(request, ct);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < MaxRetries)
            {
                var delay = RetryDelaysMs[attempt];
                logger.LogWarning(
                    "Rate limited on session {SessionId}, attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms",
                    session.Id, attempt + 1, MaxRetries, delay);
                await Task.Delay(delay, ct);
            }
        }

        throw new HttpRequestException("Rate limited by LLM API after maximum retries. Please try again later.");
    }

    private async Task<DispatchSession?> CheckLlmDisabledAsync(
        DispatchAgentRequest request, CancellationToken ct)
    {
        if (options.Value.BypassLlmGate)
            return null;

        var tenant = tenantUow.GetCurrentTenant();
        if (tenant.Settings.LlmEnabled != false)
            return null;

        logger.LogInformation("LLM is disabled for tenant {TenantId}, skipping session", request.TenantId);

        var session = new DispatchSession
        {
            Mode = request.Mode,
            TriggeredByUserId = request.TriggeredByUserId,
            StartedAt = DateTime.UtcNow
        };
        session.Fail("LLM is disabled for this tenant. Contact your administrator to enable it.");
        await tenantUow.Repository<DispatchSession>().AddAsync(session, ct);
        await tenantUow.SaveChangesAsync(ct);
        return session;
    }

    private static string SanitizeErrorMessage(Exception ex)
    {
        var message = ex.Message;
        if (ex is HttpRequestException or UnauthorizedAccessException
            || message.Contains("api key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("authentication", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
        {
            return "API authentication error. Check the LLM API key configuration.";
        }

        return message.Length > 500 ? message[..500] : message;
    }

    private async Task BroadcastSessionUpdateAsync(DispatchSession session)
    {
        try
        {
            var tenantId = tenantUow.GetCurrentTenant().Id;
            await broadcastService.BroadcastSessionUpdateAsync(tenantId, new DispatchAgentUpdateDto
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

    private async Task ReportOverageIfNeededAsync(DispatchSession session, Guid tenantId)
    {
        if (!session.IsOverage || session.Status != DispatchSessionStatus.Completed)
            return;

        try
        {
            var billingUnits = LlmPricing.GetOverageBillingUnits(session.ModelUsed ?? "");
            await stripeUsageService.ReportAiSessionOverageAsync(tenantId, billingUnits);
            logger.LogInformation("Reported AI session overage for session {SessionId} ({BillingUnits} units)",
                session.Id, billingUnits);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to report AI session overage for session {SessionId}", session.Id);
        }
    }
}
