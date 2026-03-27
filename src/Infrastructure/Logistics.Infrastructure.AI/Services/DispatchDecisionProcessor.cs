using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic.SDK.Messaging;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Processes Claude tool calls into DispatchDecision entities.
/// Handles mode-aware execution (HumanInTheLoop suggests, Autonomous executes).
/// </summary>
internal sealed class DispatchDecisionProcessor(
    IDispatchToolExecutor toolExecutor,
    ITenantUnitOfWork tenantUow,
    ITripTrackingService trackingService,
    ILogger<DispatchDecisionProcessor> logger)
{
    private static readonly HashSet<string> WriteTools =
    [
        "assign_load_to_truck",
        "create_trip",
        "dispatch_trip",
        "book_load_board_load"
    ];

    public async Task<List<ContentBase>> ProcessToolCallsAsync(
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
            await tenantUow.Repository<DispatchDecision>().AddAsync(decision, CancellationToken.None);
            session.DecisionCount++;

            await BroadcastDecisionAsync(decision);

            toolResults.Add(new ToolResultContent
            {
                ToolUseId = toolUse.Id,
                Content = [new TextContent { Text = toolResult }]
            });
        }

        return toolResults;
    }

    public static bool IsWriteTool(string toolName) => WriteTools.Contains(toolName);

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

    private static DispatchDecisionType MapToolToDecisionType(string toolName) => toolName switch
    {
        "assign_load_to_truck" => DispatchDecisionType.AssignLoad,
        "create_trip" => DispatchDecisionType.CreateTrip,
        "dispatch_trip" => DispatchDecisionType.DispatchTrip,
        "book_load_board_load" => DispatchDecisionType.BookLoadBoardLoad,
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
            await trackingService.BroadcastDispatchDecisionAsync(tenantId, decision.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast dispatch decision {DecisionId}", decision.Id);
        }
    }
}
