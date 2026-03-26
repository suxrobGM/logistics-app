using Anthropic.SDK.Messaging;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class ClaudeDispatchAgentService(
    IOptions<ClaudeOptions> options,
    DispatchConversationBuilder conversationBuilder,
    DispatchDecisionProcessor decisionProcessor,
    ITenantUnitOfWork tenantUow,
    ITripTrackingService trackingService,
    IStripeUsageService stripeUsageService,
    ILogger<ClaudeDispatchAgentService> logger) : IDispatchAgentService
{
    private const int MaxIterations = 25;

    public async Task<DispatchSession> RunAsync(DispatchAgentRequest request, CancellationToken ct = default)
    {
        var session = new DispatchSession
        {
            Mode = request.Mode,
            TriggeredByUserId = request.TriggeredByUserId,
            StartedAt = DateTime.UtcNow,
            IsOverage = request.IsOverage
        };

        await tenantUow.Repository<DispatchSession>().AddAsync(session);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Starting dispatch agent session {SessionId} in {Mode} mode (triggered by {UserId})",
            session.Id, request.Mode, request.TriggeredByUserId?.ToString() ?? "background-job");

        try
        {
            await RunAgentLoopAsync(session, request, ct);
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
        var (client, parameters, messages) = conversationBuilder.Build(session, request.Mode, config);

        var totalInputTokens = 0;
        var totalOutputTokens = 0;

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();

            logger.LogDebug("Agent loop iteration {Iteration} for session {SessionId}",
                iteration, session.Id);

            var response = await client.Messages.GetClaudeMessageAsync(parameters, ct);

            TrackTokenUsage(session, response, ref totalInputTokens, ref totalOutputTokens);
            messages.Add(response.Message);

            var textContent = response.Content?.OfType<TextContent>().FirstOrDefault()?.Text;
            if (textContent is not null)
                session.Summary = textContent;

            var toolUseBlocks = response.Content?.OfType<ToolUseContent>().ToList() ?? [];
            if (response.StopReason == "end_turn" || toolUseBlocks.Count == 0)
            {
                logger.LogInformation(
                    "Agent session {SessionId} completed after {Iterations} iterations, {Tokens} tokens",
                    session.Id, iteration + 1, session.TotalTokensUsed);
                break;
            }

            var toolResults = await decisionProcessor.ProcessToolCallsAsync(
                session, request.Mode, toolUseBlocks, textContent, ct);

            messages.Add(new Message { Role = RoleType.User, Content = toolResults });
            await tenantUow.SaveChangesAsync(ct);
        }
    }

    private static void TrackTokenUsage(
        DispatchSession session,
        MessageResponse response,
        ref int totalInputTokens,
        ref int totalOutputTokens)
    {
        totalInputTokens += response.Usage?.InputTokens ?? 0;
        totalOutputTokens += response.Usage?.OutputTokens ?? 0;
        session.TotalTokensUsed = totalInputTokens + totalOutputTokens;
    }

    private static string SanitizeErrorMessage(Exception ex)
    {
        var message = ex.Message;
        if (ex is HttpRequestException or UnauthorizedAccessException
            || message.Contains("api key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("authentication", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
        {
            return "API authentication error. Check the Claude API key configuration.";
        }

        return message.Length > 500 ? message[..500] : message;
    }

    private async Task BroadcastSessionUpdateAsync(DispatchSession session)
    {
        try
        {
            var tenantId = tenantUow.GetCurrentTenant().Id;
            await trackingService.BroadcastDispatchAgentUpdateAsync(tenantId, new DispatchAgentUpdateDto
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
            await stripeUsageService.ReportAiSessionOverageAsync(tenantId);
            logger.LogInformation("Reported AI session overage for session {SessionId}", session.Id);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to report AI session overage for session {SessionId}", session.Id);
        }
    }
}
