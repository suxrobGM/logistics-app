using System.Diagnostics;
using System.Net;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.AI.Models;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Application.Abstractions.AI;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// The provider-agnostic agent iteration loop, shared by the dispatch agent and the copilot.
/// New messages accumulate on <see cref="LlmConversation.Messages"/> - callers that persist the
/// transcript read them from there after the run.
/// </summary>
internal sealed class AgentLoopRunner(
    AIDispatchDecisionProcessor decisionProcessor,
    ILogger<AgentLoopRunner> logger)
{
    private const int MaxIterations = 25;
    private const int MaxRetries = 3;
    private static readonly int[] RetryDelaysMs = [2000, 4000, 8000];

    public async Task RunAsync(
        AIDispatchSession session,
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
        AIDispatchSession session,
        LlmConversation conversation,
        ToolCallContext toolContext,
        Func<Task>? onIterationCompleted,
        CancellationToken ct)
    {
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

    /// <summary>Strips provider auth details before the message lands on a tenant-visible session row.</summary>
    internal static string SanitizeErrorMessage(Exception ex)
    {
        // Our own message, already safe - and it must not be swallowed by the auth arm below,
        // which would send an operator hunting for a key problem during a rate-limit window.
        if (ex is LlmRateLimitedException)
            return ex.Message;

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

    private async Task<LlmResponse> SendWithRetryAsync(
        ILlmProvider provider,
        LlmRequest request,
        AIDispatchSession session,
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
                // escape - SanitizeErrorMessage cannot tell that one apart from an auth failure.
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
