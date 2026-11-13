using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Modules.AI.AIDispatch.Services;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>Executes or records the agent's tool calls as decisions. Whether a tool writes data or needs a permission comes from its metadata, not from a list of tool names.</summary>
internal sealed class AgentDecisionProcessor(
    IAgentToolExecutor toolExecutor,
    IAgentToolRegistry toolRegistry,
    ITenantUnitOfWork tenantUow,
    IAIDispatchBroadcastService broadcastService,
    IAgentDecisionDtoMapper decisionDtoMapper,
    IAgentTrustService trustService,
    IAgentRunContext runContext,
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

        foreach (var dto in await decisionDtoMapper.MapAsync(decisions, ct))
        {
            await BroadcastDecisionAsync(dto, context);
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

        var canAutoExecute = isWriteTool
            && session.Conversation.Kind == AgentConversationKind.Dispatch
            && await trustService.CanAutoExecuteAsync(definition!, ct);

        if (canAutoExecute)
        {
            return await AutoExecuteAsync(session, decision, toolCall.Name, toolInputJson, ct);
        }

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
            return Fail(session, decision, toolCall.Name, ex);
        }
    }

    private async Task<string> AutoExecuteAsync(
        AgentSession session, AgentDecision decision, string toolName, string toolInputJson, CancellationToken ct)
    {
        runContext.SetDecision(decision.Id);
        try
        {
            var result = await toolExecutor.ExecuteToolAsync(toolName, toolInputJson, ct);
            var output = ToolOutput.Parse(result);
            if (ToolOutput.IsFailure(output))
            {
                decision.MarkFailed(result);
                return result;
            }

            decision.ToolOutput = result;
            decision.MarkAutoExecuted();
            decision.TripId ??= output?.GetGuid("trip_id");
            decision.InvoiceId ??= output?.GetGuid("invoice_id");
            logger.LogInformation("Session {SessionId}: tool {ToolName} auto-executed under the tenant opt-in",
                session.Id, toolName);
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Fail(session, decision, toolName, ex);
        }
        finally
        {
            runContext.SetDecision(null);
        }
    }

    /// <summary>Marks the decision failed and returns the error JSON. The exception text is sanitized, since it is stored on a tenant-visible row and sent back to the model.</summary>
    private string Fail(AgentSession session, AgentDecision decision, string toolName, Exception ex)
    {
        logger.LogWarning(ex, "Session {SessionId}: tool {ToolName} failed", session.Id, toolName);
        var errorResult = JsonSerializer.Serialize(new { error = ToolErrorSanitizer.Sanitize(ex) });
        decision.MarkFailed(errorResult);
        return errorResult;
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

    /// <summary>Copies entity ids (load, truck, trip, ...) from the tool input onto the decision for audit links. The input keys come from the tool's input type, so renaming a property keeps the link.</summary>
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

    private async Task BroadcastDecisionAsync(AgentDecisionDto decision, ToolCallContext context)
    {
        try
        {
            if (context.DecisionBroadcastOverride is not null)
            {
                await context.DecisionBroadcastOverride(decision);
                return;
            }

            var tenantId = tenantUow.GetCurrentTenant().Id;
            await broadcastService.BroadcastDecisionAsync(tenantId, decision);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast dispatch decision {DecisionId}", decision.Id);
        }
    }
}
