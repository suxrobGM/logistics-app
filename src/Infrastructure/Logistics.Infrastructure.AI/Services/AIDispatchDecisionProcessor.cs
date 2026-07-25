using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Models;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Mappings;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Processes LLM tool calls into AIDispatchDecision entities.
/// Handles mode-aware execution (HumanInTheLoop suggests, Autonomous executes).
/// </summary>
internal sealed class AIDispatchDecisionProcessor(
    IAIDispatchToolExecutor toolExecutor,
    ITenantUnitOfWork tenantUow,
    IAIDispatchBroadcastService broadcastService,
    ILogger<AIDispatchDecisionProcessor> logger)
{
    private static readonly HashSet<string> WriteTools =
    [
        "assign_load_to_truck",
        "create_trip",
        "dispatch_trip",
        "book_loadboard_load"
    ];

    public async Task<List<LlmToolResultBlock>> ProcessToolCallsAsync(
        AIDispatchSession session,
        AIDispatchMode mode,
        List<LlmToolUseBlock> toolCalls,
        string? reasoning,
        CancellationToken ct)
    {
        var toolResults = new List<LlmToolResultBlock>();
        var decisions = new List<AIDispatchDecision>();

        foreach (var toolCall in toolCalls)
        {
            var decision = CreateDecision(session, toolCall, reasoning);
            var toolResult = await ExecuteOrSuggestAsync(session, decision, toolCall, mode, ct);

            ExtractEntityIds(decision, toolCall.Input);
            await tenantUow.Repository<AIDispatchDecision>().AddAsync(decision, CancellationToken.None);
            session.DecisionCount++;
            decisions.Add(decision);

            toolResults.Add(new LlmToolResultBlock(toolCall.Id, toolResult));
        }

        // Save all decisions first, then broadcast - ensures clients see committed data
        await tenantUow.SaveChangesAsync(ct);

        foreach (var decision in decisions)
        {
            await BroadcastDecisionAsync(decision);
        }

        return toolResults;
    }

    public static bool IsWriteTool(string toolName) => WriteTools.Contains(toolName);

    private async Task<string> ExecuteOrSuggestAsync(
        AIDispatchSession session,
        AIDispatchDecision decision,
        LlmToolUseBlock toolCall,
        AIDispatchMode mode,
        CancellationToken ct)
    {
        var toolInputJson = toolCall.Input?.ToJsonString() ?? "{}";
        var isWriteTool = WriteTools.Contains(toolCall.Name);

        if (isWriteTool && mode == AIDispatchMode.HumanInTheLoop)
        {
            decision.Status = AIDispatchDecisionStatus.Suggested;
            var result = JsonSerializer.Serialize(new
            {
                status = "suggested",
                message = "This action has been recorded as a suggestion for dispatcher approval."
            });
            decision.ToolOutput = result;
            logger.LogInformation("Session {SessionId}: tool {ToolName} queued as suggestion",
                session.Id, toolCall.Name);
            return result;
        }

        try
        {
            var result = await toolExecutor.ExecuteToolAsync(toolCall.Name, toolInputJson, ct);
            decision.ToolOutput = result;

            if (isWriteTool)
            {
                decision.MarkExecuted();
                logger.LogInformation("Session {SessionId}: write tool {ToolName} executed successfully",
                    session.Id, toolCall.Name);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Session {SessionId}: tool {ToolName} failed",
                session.Id, toolCall.Name);
            var errorResult = JsonSerializer.Serialize(new { error = ex.Message });
            decision.MarkFailed(errorResult);
            return errorResult;
        }
    }

    private static AIDispatchDecision CreateDecision(
        AIDispatchSession session,
        LlmToolUseBlock toolCall,
        string? reasoning)
    {
        return new AIDispatchDecision
        {
            SessionId = session.Id,
            Type = MapToolToDecisionType(toolCall.Name),
            ToolName = toolCall.Name,
            ToolInput = toolCall.Input?.ToJsonString() ?? "{}",
            Reasoning = reasoning ?? ""
        };
    }

    private static AIDispatchDecisionType MapToolToDecisionType(string toolName) => toolName switch
    {
        "assign_load_to_truck" => AIDispatchDecisionType.AssignLoad,
        "create_trip" => AIDispatchDecisionType.CreateTrip,
        "dispatch_trip" => AIDispatchDecisionType.DispatchTrip,
        "book_loadboard_load" => AIDispatchDecisionType.BookLoadBoardLoad,
        _ => AIDispatchDecisionType.Query
    };

    private static void ExtractEntityIds(AIDispatchDecision decision, JsonNode? input)
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

    private async Task BroadcastDecisionAsync(AIDispatchDecision decision)
    {
        try
        {
            var tenantId = tenantUow.GetCurrentTenant().Id;
            await broadcastService.BroadcastDecisionAsync(tenantId, decision.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast dispatch decision {DecisionId}", decision.Id);
        }
    }
}
