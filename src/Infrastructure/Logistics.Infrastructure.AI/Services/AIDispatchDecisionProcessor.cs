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
/// Write/permission/decision-type behavior is driven entirely by the tool's
/// <see cref="AIDispatchToolDefinition"/> metadata - there is no separate tool-name list here.
/// </summary>
internal sealed class AIDispatchDecisionProcessor(
    IAIDispatchToolExecutor toolExecutor,
    IAIDispatchToolRegistry toolRegistry,
    ITenantUnitOfWork tenantUow,
    IAIDispatchBroadcastService broadcastService,
    ILogger<AIDispatchDecisionProcessor> logger)
{
    public async Task<List<LlmToolResultBlock>> ProcessToolCallsAsync(
        AIDispatchSession session,
        ToolCallContext context,
        List<LlmToolUseBlock> toolCalls,
        string? reasoning,
        CancellationToken ct)
    {
        var toolResults = new List<LlmToolResultBlock>();
        var decisions = new List<AIDispatchDecision>();

        foreach (var toolCall in toolCalls)
        {
            var definition = toolRegistry.TryGetDefinition(toolCall.Name);
            var decision = CreateDecision(session, toolCall, definition, reasoning);
            var toolResult = await ExecuteOrSuggestAsync(session, decision, toolCall, definition, context, ct);

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
            await BroadcastDecisionAsync(decision, context);
        }

        return toolResults;
    }

    private async Task<string> ExecuteOrSuggestAsync(
        AIDispatchSession session,
        AIDispatchDecision decision,
        LlmToolUseBlock toolCall,
        AIDispatchToolDefinition? definition,
        ToolCallContext context,
        CancellationToken ct)
    {
        var toolInputJson = toolCall.Input?.ToJsonString() ?? "{}";

        if (context.CallerPermissions is not null
            && definition?.RequiredPermission is { } requiredPermission
            && !context.CallerPermissions.Contains(requiredPermission))
        {
            var denied = JsonSerializer.Serialize(new
            {
                error = "permission_denied",
                required_permission = requiredPermission
            });
            decision.MarkFailed(denied);
            logger.LogInformation("Session {SessionId}: tool {ToolName} denied - caller lacks {Permission}",
                session.Id, toolCall.Name, requiredPermission);
            return denied;
        }

        var isWriteTool = definition?.IsWrite == true;

        if (isWriteTool && context.Mode == AIDispatchMode.HumanInTheLoop)
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
        AIDispatchToolDefinition? definition,
        string? reasoning)
    {
        return new AIDispatchDecision
        {
            SessionId = session.Id,
            Type = definition?.DecisionType ?? AIDispatchDecisionType.Query,
            ToolName = toolCall.Name,
            ToolInput = toolCall.Input?.ToJsonString() ?? "{}",
            Reasoning = reasoning ?? ""
        };
    }

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

        if (input["invoice_id"] is JsonValue invoiceIdVal && Guid.TryParse(invoiceIdVal.GetValue<string>(), out var invoiceId))
            decision.InvoiceId = invoiceId;

        if (input["customer_id"] is JsonValue customerIdVal && Guid.TryParse(customerIdVal.GetValue<string>(), out var customerId))
            decision.CustomerId = customerId;
    }

    private async Task BroadcastDecisionAsync(AIDispatchDecision decision, ToolCallContext context)
    {
        try
        {
            if (context.DecisionBroadcastOverride is not null)
            {
                await context.DecisionBroadcastOverride(decision.ToDto());
                return;
            }

            var tenantId = tenantUow.GetCurrentTenant().Id;
            await broadcastService.BroadcastDecisionAsync(tenantId, decision.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast dispatch decision {DecisionId}", decision.Id);
        }
    }
}
