using Logistics.Domain.Entities;
using Logistics.Infrastructure.AI.Models;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Application.Abstractions.AI;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// The provider-agnostic agent iteration loop, shared by the dispatch agent and the copilot:
/// send, accumulate tokens onto the session, process tool calls, repeat until the model stops.
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
        LlmOptions config,
        Func<Task>? onIterationCompleted,
        CancellationToken ct)
    {
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
                session, toolContext, result.ToolCalls, result.TextContent, ct);

            conversation.Messages.Add(LlmMessage.FromToolResults(toolResults));

            if (onIterationCompleted is not null)
                await onIterationCompleted();
        }

        var modelUsed = session.ModelUsed ?? config.GetProviderConfig(config.DefaultProvider).Model;
        session.RequestCost = LlmPricing.GetMultiplier(modelUsed);
        session.EstimatedCostUsd = LlmPricing.Calculate(
            modelUsed,
            totalInputTokens, totalOutputTokens,
            totalCacheReadTokens, totalCacheCreationTokens);
    }

    /// <summary>
    /// Strips provider secrets and auth details out of an exception before it lands on a
    /// tenant-visible session row.
    /// </summary>
    internal static string SanitizeErrorMessage(Exception ex)
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
}
