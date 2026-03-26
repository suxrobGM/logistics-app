using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Tool = Anthropic.SDK.Common.Tool;
using Function = Anthropic.SDK.Common.Function;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class ClaudeDispatchAgentService(
    IOptions<ClaudeOptions> options,
    IDispatchToolRegistry toolRegistry,
    IDispatchToolExecutor toolExecutor,
    ITenantUnitOfWork tenantUow,
    ILogger<ClaudeDispatchAgentService> logger) : IDispatchAgentService
{
    private static readonly HashSet<string> WriteTools =
    [
        "assign_load_to_truck",
        "create_trip",
        "dispatch_trip",
        "book_load_board_load"
    ];

    private const int MaxIterations = 25;

    public async Task<DispatchSession> RunAsync(DispatchAgentRequest request, CancellationToken ct = default)
    {
        var session = new DispatchSession
        {
            Mode = request.Mode,
            TriggeredByUserId = request.TriggeredByUserId,
            StartedAt = DateTime.UtcNow
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
            session.Fail(ex.Message);
        }

        await tenantUow.SaveChangesAsync(ct);
        return session;
    }

    public async Task CancelAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await tenantUow.Repository<DispatchSession>().GetByIdAsync(sessionId);
        if (session is null || session.Status != DispatchSessionStatus.Running)
            return;

        session.Cancel();
        await tenantUow.SaveChangesAsync(ct);
    }

    private async Task RunAgentLoopAsync(
        DispatchSession session,
        DispatchAgentRequest request,
        CancellationToken ct)
    {
        var config = options.Value;
        var (client, parameters, messages) = BuildConversation(session, request.Mode, config);

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

            var toolResults = await ProcessToolCallsAsync(
                session, request.Mode, toolUseBlocks, textContent, ct);

            messages.Add(new Message { Role = RoleType.User, Content = toolResults });
            await tenantUow.SaveChangesAsync(ct);
        }
    }

    private (AnthropicClient client, MessageParameters parameters, List<Message> messages)
        BuildConversation(DispatchSession session, DispatchAgentMode mode, ClaudeOptions config)
    {
        var systemPrompt = DispatchSystemPrompt.Build("Fleet", mode);

        var tools = toolRegistry.GetToolDefinitions()
            .Select<DispatchToolDefinition, Tool>(t =>
                new Function(t.Name, t.Description, (JsonNode)t.InputSchema))
            .ToList();

        logger.LogInformation(
            "Agent session {SessionId} initialized with {ToolCount} tools, model {Model}",
            session.Id, tools.Count, config.Model);

        var messages = new List<Message>
        {
            new(RoleType.User,
                "Analyze the current fleet state and optimize dispatch assignments. " +
                "Start by getting a fleet overview, then process all unassigned loads.")
        };

        var client = new AnthropicClient(config.ApiKey);

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = config.MaxTokens,
            Model = config.Model,
            Stream = false,
            Temperature = 0m,
            System = [new SystemMessage(systemPrompt)],
            Tools = tools
        };

        return (client, parameters, messages);
    }

    private async Task<List<ContentBase>> ProcessToolCallsAsync(
        DispatchSession session,
        DispatchAgentMode mode,
        List<ToolUseContent> toolUseBlocks,
        string? reasoning,
        CancellationToken ct)
    {
        var toolResults = new List<ContentBase>();

        foreach (var toolUse in toolUseBlocks)
        {
            var decision = CreateDecision(session, toolUse, reasoning);
            var toolResult = await ExecuteOrSuggestAsync(session, decision, toolUse, mode, ct);

            ExtractEntityIds(decision, toolUse.Input);
            await tenantUow.Repository<DispatchDecision>().AddAsync(decision);
            session.DecisionCount++;

            toolResults.Add(new ToolResultContent
            {
                ToolUseId = toolUse.Id,
                Content = [new TextContent { Text = toolResult }]
            });
        }

        return toolResults;
    }

    private async Task<string> ExecuteOrSuggestAsync(
        DispatchSession session,
        DispatchDecision decision,
        ToolUseContent toolUse,
        DispatchAgentMode mode,
        CancellationToken ct)
    {
        var toolInputJson = toolUse.Input?.ToJsonString() ?? "{}";
        var isWriteTool = WriteTools.Contains(toolUse.Name);

        if (isWriteTool && mode == DispatchAgentMode.HumanInTheLoop)
        {
            decision.Status = DispatchDecisionStatus.Suggested;
            var result = JsonSerializer.Serialize(new
            {
                status = "suggested",
                message = "This action has been recorded as a suggestion for dispatcher approval."
            });
            decision.ToolOutput = result;
            logger.LogInformation("Session {SessionId}: tool {ToolName} queued as suggestion",
                session.Id, toolUse.Name);
            return result;
        }

        try
        {
            var result = await toolExecutor.ExecuteToolAsync(toolUse.Name, toolInputJson, ct);
            decision.ToolOutput = result;

            if (isWriteTool)
            {
                decision.Status = DispatchDecisionStatus.Executed;
                decision.MarkExecuted();
                logger.LogInformation("Session {SessionId}: write tool {ToolName} executed successfully",
                    session.Id, toolUse.Name);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Session {SessionId}: tool {ToolName} failed",
                session.Id, toolUse.Name);
            var errorResult = JsonSerializer.Serialize(new { error = ex.Message });
            decision.MarkFailed(errorResult);
            return errorResult;
        }
    }

    private static DispatchDecision CreateDecision(
        DispatchSession session,
        ToolUseContent toolUse,
        string? reasoning)
    {
        return new DispatchDecision
        {
            SessionId = session.Id,
            Type = MapToolToDecisionType(toolUse.Name),
            ToolName = toolUse.Name,
            ToolInput = toolUse.Input?.ToJsonString() ?? "{}",
            Reasoning = reasoning ?? ""
        };
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

    private static DispatchDecisionType MapToolToDecisionType(string toolName) => toolName switch
    {
        "assign_load_to_truck" => DispatchDecisionType.AssignLoad,
        "create_trip" => DispatchDecisionType.CreateTrip,
        "dispatch_trip" => DispatchDecisionType.DispatchTrip,
        "book_load_board_load" => DispatchDecisionType.BookLoadBoardLoad,
        _ => DispatchDecisionType.AssignLoad
    };

    private static void ExtractEntityIds(DispatchDecision decision, JsonNode? input)
    {
        if (input is null)
            return;

        if (input["load_id"] is JsonValue loadIdVal && Guid.TryParse(loadIdVal.GetValue<string>(), out var loadId))
            decision.LoadId = loadId;

        if (input["truck_id"] is JsonValue truckIdVal && Guid.TryParse(truckIdVal.GetValue<string>(), out var truckId))
            decision.TruckId = truckId;

        if (input["trip_id"] is JsonValue tripIdVal && Guid.TryParse(tripIdVal.GetValue<string>(), out var tripId))
            decision.TripId = tripId;
    }
}
