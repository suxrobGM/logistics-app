using Logistics.Application.Abstractions.Agents;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Mappings;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Processes LLM tool calls into AgentDecision entities.
/// Write tools always become Suggested decisions awaiting dispatcher approval; read tools execute
/// immediately. Write/permission/decision-type behavior comes from the tool's
/// <see cref="AgentToolDefinition"/> metadata - there is no tool-name list here.
/// </summary>
internal sealed class AgentDecisionProcessor(
    IAgentToolExecutor toolExecutor,
    IAgentToolRegistry toolRegistry,
    ITenantUnitOfWork tenantUow,
    IAIDispatchBroadcastService broadcastService,
    ILogger<AgentDecisionProcessor> logger)
{
    public async Task<List<LlmToolResultBlock>> ProcessToolCallsAsync(
        AgentSession session,
        ToolCallContext context,
        List<LlmToolUseBlock> toolCalls,
        string? reasoning,
        CancellationToken ct)
    {
        var toolResults = new List<LlmToolResultBlock>();
        var decisions = new List<AgentDecision>();

        foreach (var toolCall in toolCalls)
        {
            var definition = toolRegistry.TryGetDefinition(toolCall.Name);
            var decision = CreateDecision(session, toolCall, definition, reasoning);
            var toolResult = await ExecuteOrSuggestAsync(session, decision, toolCall, definition, context, ct);

            ExtractEntityIds(decision, toolCall.Input);
            await tenantUow.Repository<AgentDecision>().AddAsync(decision, CancellationToken.None);
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
        AgentSession session,
        AgentDecision decision,
        LlmToolUseBlock toolCall,
        AgentToolDefinition? definition,
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

        if (isWriteTool)
        {
            decision.Status = AgentDecisionStatus.Suggested;
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
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Session {SessionId}: tool {ToolName} failed",
                session.Id, toolCall.Name);

            // Sanitized: this lands on a tenant-visible decision row AND goes back into the model's
            // context, so a raw exception could leak connection strings or vendor credentials.
            var errorResult = JsonSerializer.Serialize(
                new { error = LlmErrorSanitizer.ForToolCall(ex) });
            decision.MarkFailed(errorResult);
            return errorResult;
        }
    }

    private static AgentDecision CreateDecision(
        AgentSession session,
        LlmToolUseBlock toolCall,
        AgentToolDefinition? definition,
        string? reasoning)
    {
        return new AgentDecision
        {
            SessionId = session.Id,
            Type = definition?.DecisionType ?? AgentDecisionType.Query,
            ToolName = toolCall.Name,
            ToolInput = toolCall.Input?.ToJsonString() ?? "{}",
            Reasoning = reasoning ?? ""
        };
    }

    /// <summary>
    /// The keys come from the tool's own input type, so renaming a property moves the audit link
    /// with it rather than dropping it here without a word.
    /// </summary>
    private static void ExtractEntityIds(AgentDecision decision, JsonNode? input)
    {
        if (input is null)
            return;

        foreach (var (kind, key) in AgentToolCatalog.EntityIdsFor(decision.ToolName!))
        {
            if (input.GetGuid(key) is not { } id)
                continue;

            switch (kind)
            {
                case AgentEntityKind.Load: decision.LoadId = id; break;
                case AgentEntityKind.Truck: decision.TruckId = id; break;
                case AgentEntityKind.Trip: decision.TripId = id; break;
                case AgentEntityKind.Invoice: decision.InvoiceId = id; break;
                case AgentEntityKind.Customer: decision.CustomerId = id; break;
                case AgentEntityKind.Negotiation: decision.NegotiationId = id; break;
            }
        }
    }

    private async Task BroadcastDecisionAsync(AgentDecision decision, ToolCallContext context)
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
