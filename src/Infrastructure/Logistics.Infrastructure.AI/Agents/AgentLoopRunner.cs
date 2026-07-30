using System.Diagnostics;
using System.Net;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// The provider-agnostic agent iteration loop, shared by the dispatch agent and the copilot.
/// New messages accumulate on <see cref="LlmConversation.Messages"/> - callers that persist the
/// transcript read them from there after the run.
/// </summary>
internal sealed class AgentLoopRunner(
    AgentDecisionProcessor decisionProcessor,
    ITenantUnitOfWork tenantUow,
    ILogger<AgentLoopRunner> logger)
{
    private const int MaxIterations = 25;
    private const int MaxRetries = 3;
    private static readonly int[] RetryDelaysMs = [2000, 4000, 8000];

    public async Task RunAsync(
        AgentSession session,
        LlmConversation conversation,
        ToolCallContext toolContext,
        Func<Task>? onIterationCompleted,
        CancellationToken ct)
    {
        try
        {
            await RunIterationsAsync(session, conversation, toolContext, onIterationCompleted, ct);
        }
        finally
        {
            // In a finally so a failed or cancelled session still reports what it burned. The
            // audit trail promises tokens *and* cost; recording 240k tokens at $0.00 is a lie, and
            // quota that only counts successes is free retries on a failing prompt.
            session.RequestCost = LlmPricing.GetMultiplier(conversation.Model);
            session.EstimatedCostUsd = LlmPricing.Calculate(
                conversation.Model,
                session.InputTokensUsed, session.OutputTokensUsed,
                session.CacheReadTokens, session.CacheCreationTokens);
        }
    }

    private async Task RunIterationsAsync(
        AgentSession session,
        LlmConversation conversation,
        ToolCallContext toolContext,
        Func<Task>? onIterationCompleted,
        CancellationToken ct)
    {
        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();
            await ThrowIfCancelledElsewhereAsync(session, ct);

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

            session.InputTokensUsed += result.Usage.InputTokens;
            session.OutputTokensUsed += result.Usage.OutputTokens;
            session.CacheReadTokens += result.Usage.CacheReadTokens;
            session.CacheCreationTokens += result.Usage.CacheCreationTokens;

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
                session, toolContext, result.ToolCalls, result.TextContent, ct);

            conversation.Messages.Add(LlmMessage.FromToolResults(toolResults));

            if (onIterationCompleted is not null)
                await onIterationCompleted();
        }
    }

    /// <summary>
    /// Honours a cancel issued on a different instance. AgentSessionCancellationRegistry is
    /// process-local, but a session runs in a Hangfire worker while the cancel arrives at whichever
    /// API instance the request landed on - there, TryCancel finds nothing and the worker would
    /// keep burning tokens against a session the database already shows as Cancelled.
    /// </summary>
    private async Task ThrowIfCancelledElsewhereAsync(AgentSession session, CancellationToken ct)
    {
        // Deliberately AsNoTracking and projected: reading session.Status off the tracked entity
        // returns this worker's own cached value, which is always Running.
        var status = await tenantUow.Repository<AgentSession>().Query()
            .AsNoTracking()
            .Where(s => s.Id == session.Id)
            .Select(s => (AgentSessionStatus?)s.Status)
            .FirstOrDefaultAsync(ct);

        if (status is null or AgentSessionStatus.Running)
            return;

        logger.LogInformation(
            "Agent session {SessionId} is {Status} in the database - stopping this worker",
            session.Id, status);
        throw new OperationCanceledException($"Session {session.Id} was {status} elsewhere.");
    }

    private async Task<LlmResponse> SendWithRetryAsync(
        ILlmProvider provider,
        LlmRequest request,
        AgentSession session,
        CancellationToken ct)
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await provider.SendAsync(request, ct);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Rethrow on the final attempt rather than letting the provider's own exception
                // escape - LlmErrorSanitizer cannot tell that one apart from an auth failure.
                if (attempt == MaxRetries)
                    throw new LlmRateLimitedException(
                        "Rate limited by the LLM API after maximum retries. Please try again later.", ex);

                var delay = RetryDelaysMs[attempt];
                logger.LogWarning(
                    "Rate limited on session {SessionId}, attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms",
                    session.Id, attempt + 1, MaxRetries, delay);
                await Task.Delay(delay, ct);
            }
        }

        throw new UnreachableException("The retry loop either returns a response or throws.");
    }
}
