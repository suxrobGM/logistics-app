using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Mappings;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Processes LLM tool calls into DispatchDecision entities.
/// Handles mode-aware execution (HumanInTheLoop suggests, Autonomous executes).
/// </summary>
internal sealed class DispatchDecisionProcessor(
    IDispatchToolExecutor toolExecutor,
    ITenantUnitOfWork tenantUow,
    IDispatchAgentBroadcastService broadcastService,
    ILogger<DispatchDecisionProcessor> logger)
{
    private static readonly HashSet<string> WriteTools =
    [
        "assign_load_to_truck",
        "create_trip",
        "dispatch_trip",
        "book_loadboard_load"
    ];

    public async Task<List<LlmToolResultBlock>> ProcessToolCallsAsync(
        DispatchSession session,
        DispatchAgentMode mode,
        List<LlmToolUseBlock> toolCalls,
        string? reasoning,
        CancellationToken ct)
    {
        var toolResults = new List<LlmToolResultBlock>();
        var decisions = new List<DispatchDecision>();

        foreach (var toolCall in toolCalls)
        {
            var decision = CreateDecision(session, toolCall, reasoning);
            var toolResult = await ExecuteOrSuggestAsync(session, decision, toolCall, mode, ct);

            ExtractEntityIds(decision, toolCall.Input);
            await tenantUow.Repository<DispatchDecision>().AddAsync(decision, CancellationToken.None);
            session.DecisionCount++;
            decisions.Add(decision);

            toolResults.Add(new LlmToolResultBlock(toolCall.Id, toolResult));
        }

        // Save all decisions first, then broadcast — ensures clients see committed data
        await tenantUow.SaveChangesAsync(ct);

        foreach (var decision in decisions)
        {
            await BroadcastDecisionAsync(decision);
        }

        return toolResults;
    }

    public static bool IsWriteTool(string toolName) => WriteTools.Contains(toolName);

    private async Task<string> ExecuteOrSuggestAsync(
        DispatchSession session,
        DispatchDecision decision,
        LlmToolUseBlock toolCall,
        DispatchAgentMode mode,
        CancellationToken ct)
    {
        var toolInputJson = toolCall.Input?.ToJsonString() ?? "{}";
        var isWriteTool = WriteTools.Contains(toolCall.Name);

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

    private static DispatchDecision CreateDecision(
        DispatchSession session,
        LlmToolUseBlock toolCall,
        string? reasoning)
    {
        return new DispatchDecision
        {
            SessionId = session.Id,
            Type = MapToolToDecisionType(toolCall.Name),
            ToolName = toolCall.Name,
            ToolInput = toolCall.Input?.ToJsonString() ?? "{}",
            Reasoning = reasoning ?? ""
        };
    }

    private static DispatchDecisionType MapToolToDecisionType(string toolName) => toolName switch
    {
        "assign_load_to_truck" => DispatchDecisionType.AssignLoad,
        "create_trip" => DispatchDecisionType.CreateTrip,
        "dispatch_trip" => DispatchDecisionType.DispatchTrip,
        "book_loadboard_load" => DispatchDecisionType.BookLoadBoardLoad,
        _ => DispatchDecisionType.Query
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

    private async Task BroadcastDecisionAsync(DispatchDecision decision)
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
