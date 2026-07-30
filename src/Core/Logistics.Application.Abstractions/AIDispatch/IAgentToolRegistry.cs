using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
/// Registry of all tools available to the AI agents (dispatch and copilot).
/// Tool definitions are declared once and reused by the agent loop and the MCP server.
/// </summary>
public interface IAgentToolRegistry
{
    /// <summary>
    /// The catalogue for one agent run. Tools whose feature is off are dropped (their schemas cost
    /// tokens every request), as are tools the caller lacks the permission for, so the model never
    /// sees a tool it cannot use. Null <paramref name="callerPermissions"/> skips that filter.
    /// </summary>
    IReadOnlyList<AIDispatchToolDefinition> GetToolDefinitions(
        IReadOnlySet<TenantFeature> enabledFeatures,
        IReadOnlySet<string>? callerPermissions = null,
        bool forDispatchAgent = false);

    /// <summary>Every tool, gated or not. For surfaces that gate per call rather than per catalogue.</summary>
    IReadOnlyList<AIDispatchToolDefinition> GetAllToolDefinitions();

    /// <summary>Null for unknown (e.g. hallucinated) tool names.</summary>
    AIDispatchToolDefinition? TryGetDefinition(string name);
}

/// <summary>
/// A single tool the AI agents can use. Compatible with both Claude API tool schemas and MCP.
/// </summary>
/// <param name="RequiredFeature">
/// Gates the tool. Read by both the schema filter and the MCP call-time check, so a new gated
/// group is one field rather than a flag threaded through every caller.
/// </param>
/// <param name="IsWrite">
/// Write tools mutate state: HumanInTheLoop turns them into Suggested decisions, Autonomous
/// executes them. The single registration point - there is no separate write-tool list.
/// </param>
/// <param name="RequiredPermission">
/// Enforced only for permission-scoped runs (copilot); dispatch runs are gated by the endpoint's
/// policy instead.
/// </param>
/// <param name="DecisionType">How a call is categorized in the decision audit trail.</param>
/// <param name="DispatchAgent">
/// Whether the fleet dispatch agent may call this tool. False by default: that agent can run
/// Autonomous, so a tool it should not have executes unattended. The copilot is unaffected.
/// </param>
public record AIDispatchToolDefinition(
    string Name,
    string Description,
    JsonNode InputSchema,
    TenantFeature? RequiredFeature = null,
    bool IsWrite = false,
    string? RequiredPermission = null,
    AIDispatchDecisionType DecisionType = AIDispatchDecisionType.Query,
    bool DispatchAgent = false);
