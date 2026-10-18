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
/// <remarks>
/// No retry here: both vendor SDKs already retry 429 and 5xx with backoff, and a second layer would
/// multiply the attempts. A failure that reaches this loop is final.
/// </remarks>
internal sealed class AgentLoopRunner(
    AgentDecisionProcessor decisionProcessor,
    ITenantUnitOfWork tenantUow,
    ILogger<AgentLoopRunner> logger)
{
    private const int MaxIterations = 25;

    /// <summary>Appended when a reply hits the output cap, so neither the user nor the next turn reads it as complete.</summary>
    internal const string TruncatedNotice = "[Reply cut off at the output length limit.]";

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
            // In a finally so failed/cancelled sessions still record what they burned - the
            // budget counts every session, else failing prompts would be free retries.
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
                Effort = conversation.Effort
            };

            var result = await conversation.Provider.SendAsync(llmRequest, ct);

            session.InputTokensUsed += result.Usage.InputTokens;
            session.OutputTokensUsed += result.Usage.OutputTokens;
            session.CacheReadTokens += result.Usage.CacheReadTokens;
            session.CacheCreationTokens += result.Usage.CacheCreationTokens;

            if (result.StopReason == LlmStopReason.MaxTokens)
            {
                conversation.Messages.Add(Truncated(result.AssistantMessage));
                session.Summary = result.TextContent is null
                    ? TruncatedNotice
                    : $"{result.TextContent}\n\n{TruncatedNotice}";
                logger.LogWarning(
                    "Agent session {SessionId} hit the {MaxTokens}-token output cap on iteration {Iteration}",
                    session.Id, conversation.MaxTokens, iteration + 1);
                break;
            }

            conversation.Messages.Add(result.AssistantMessage);

            if (result.TextContent is not null)
                session.Summary = result.TextContent;

            // The server paused its own tool loop (web search); resending the conversation as it
            // stands resumes it. No tool results to add.
            if (result.StopReason == LlmStopReason.PauseTurn)
                continue;

            if (result.ToolCalls.Count == 0)
            {
                logger.LogInformation(
                    "Agent session {SessionId} completed after {Iterations} iterations, {Tokens} tokens ({StopReason})",
                    session.Id, iteration + 1, session.TotalTokensUsed, result.StopReason);
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
    /// A cut-off reply without its tool calls: their arguments may be incomplete, so they must not
    /// run, and a tool call left without a result would make the replayed transcript invalid.
    /// </summary>
    private static LlmMessage Truncated(LlmMessage reply) =>
        new(reply.Role, [.. reply.Content.Where(b => b is not LlmToolUseBlock), new LlmTextBlock(TruncatedNotice)]);

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
}
