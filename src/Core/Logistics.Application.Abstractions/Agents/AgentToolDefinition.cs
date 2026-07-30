using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.Agents;

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
public record AgentToolDefinition(
    string Name,
    string Description,
    JsonNode InputSchema,
    TenantFeature? RequiredFeature = null,
    bool IsWrite = false,
    string? RequiredPermission = null,
    AIDispatchDecisionType DecisionType = AIDispatchDecisionType.Query,
    bool DispatchAgent = false);
