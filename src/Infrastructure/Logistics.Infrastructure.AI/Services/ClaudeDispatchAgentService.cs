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
    /// <summary>
    /// Set of tools that modify state (write operations).
    /// In HumanInTheLoop mode, these create suggestions instead of executing.
    /// </summary>
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

        try
        {
            await RunAgentLoopAsync(session, request, ct);
            session.Complete(session.Summary);
        }
        catch (OperationCanceledException)
        {
            session.Cancel();
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

        // Resolve tenant name for system prompt
        var tenantName = "Fleet"; // Default — could be resolved from tenant entity
        var systemPrompt = DispatchSystemPrompt.Build(tenantName, request.Mode);

        // Build Claude API tools from registry
        var toolDefinitions = toolRegistry.GetToolDefinitions();
        var tools = toolDefinitions.Select<DispatchToolDefinition, Tool>(t =>
            new Function(t.Name, t.Description, (JsonNode)t.InputSchema))
            .ToList();

        // Initialize conversation
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

        var totalInputTokens = 0;
        var totalOutputTokens = 0;

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();

            logger.LogDebug("Agent loop iteration {Iteration} for session {SessionId}",
                iteration, session.Id);

            var response = await client.Messages.GetClaudeMessageAsync(parameters, ct);

            // Track token usage
            totalInputTokens += response.Usage?.InputTokens ?? 0;
            totalOutputTokens += response.Usage?.OutputTokens ?? 0;
            session.TotalTokensUsed = totalInputTokens + totalOutputTokens;

            // Add assistant response to conversation
            messages.Add(response.Message);

            // Extract text content for session summary
            var textContent = response.Content?.OfType<TextContent>().FirstOrDefault()?.Text;
            if (textContent is not null)
            {
                session.Summary = textContent;
            }

            // Check if done
            var toolUseBlocks = response.Content?.OfType<ToolUseContent>().ToList() ?? [];
            if (response.StopReason == "end_turn" || toolUseBlocks.Count == 0)
            {
                logger.LogInformation(
                    "Agent session {SessionId} completed after {Iterations} iterations, {Tokens} tokens",
                    session.Id, iteration + 1, session.TotalTokensUsed);
                break;
            }

            // Process tool calls
            var toolResults = new List<ContentBase>();
            foreach (var toolUse in toolUseBlocks)
            {
                var toolInputJson = toolUse.Input?.ToJsonString() ?? "{}";
                var isWriteTool = WriteTools.Contains(toolUse.Name);

                // Record decision
                var decision = new DispatchDecision
                {
                    SessionId = session.Id,
                    Type = MapToolToDecisionType(toolUse.Name),
                    ToolName = toolUse.Name,
                    ToolInput = toolInputJson,
                    Reasoning = textContent ?? ""
                };

                string toolResult;

                if (isWriteTool && request.Mode == DispatchAgentMode.HumanInTheLoop)
                {
                    // In suggestion mode, don't execute write tools — just record the suggestion
                    decision.Status = DispatchDecisionStatus.Suggested;
                    toolResult = JsonSerializer.Serialize(new
                    {
                        status = "suggested",
                        message = "This action has been recorded as a suggestion for dispatcher approval."
                    });
                    decision.ToolOutput = toolResult;
                }
                else
                {
                    // Execute the tool (read tools always execute, write tools execute in autonomous mode)
                    try
                    {
                        toolResult = await toolExecutor.ExecuteToolAsync(toolUse.Name, toolInputJson, ct);
                        decision.ToolOutput = toolResult;

                        if (isWriteTool)
                        {
                            decision.Status = DispatchDecisionStatus.Executed;
                            decision.MarkExecuted();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Tool {ToolName} failed in session {SessionId}",
                            toolUse.Name, session.Id);
                        toolResult = JsonSerializer.Serialize(new { error = ex.Message });
                        decision.MarkFailed(toolResult);
                    }
                }

                // Extract entity IDs from tool input for the decision record
                ExtractEntityIds(decision, toolUse.Input);

                await tenantUow.Repository<DispatchDecision>().AddAsync(decision);
                session.DecisionCount++;

                toolResults.Add(new ToolResultContent
                {
                    ToolUseId = toolUse.Id,
                    Content = [new TextContent { Text = toolResult }]
                });
            }

            // Add tool results to conversation
            messages.Add(new Message
            {
                Role = RoleType.User,
                Content = toolResults
            });

            await tenantUow.SaveChangesAsync(ct);
        }
    }

    private static DispatchDecisionType MapToolToDecisionType(string toolName) => toolName switch
    {
        "assign_load_to_truck" => DispatchDecisionType.AssignLoad,
        "create_trip" => DispatchDecisionType.CreateTrip,
        "dispatch_trip" => DispatchDecisionType.DispatchTrip,
        "book_load_board_load" => DispatchDecisionType.BookLoadBoardLoad,
        _ => DispatchDecisionType.AssignLoad // Read tools default
    };

    private static void ExtractEntityIds(DispatchDecision decision, JsonNode? input)
    {
        if (input is null) return;

        if (input["load_id"] is JsonValue loadIdVal && Guid.TryParse(loadIdVal.GetValue<string>(), out var loadId))
            decision.LoadId = loadId;

        if (input["truck_id"] is JsonValue truckIdVal && Guid.TryParse(truckIdVal.GetValue<string>(), out var truckId))
            decision.TruckId = truckId;

        if (input["trip_id"] is JsonValue tripIdVal && Guid.TryParse(tripIdVal.GetValue<string>(), out var tripId))
            decision.TripId = tripId;
    }
}
