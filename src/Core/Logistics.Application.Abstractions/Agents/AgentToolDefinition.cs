using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.Agents;

/// <summary>
/// A single tool the AI agents can use. Compatible with both Claude API tool schemas and MCP.
/// </summary>
/// <remarks>
/// Only name, description and schema are positional. Everything else is an init property so that
/// every piece of behaviour metadata is named at the declaration site - a bare
/// <see cref="TenantFeature"/> in the fourth slot is otherwise easy to mistake for something else.
/// </remarks>
public record AgentToolDefinition(string Name, string Description, JsonNode InputSchema)
{
    /// <summary>
    /// Gates the tool. Read by both the schema filter and the MCP call-time check, so a new gated
    /// group is one field rather than a flag threaded through every caller.
    /// </summary>
    public TenantFeature? RequiredFeature { get; init; }

    /// <summary>
    /// Enforced only for permission-scoped runs (copilot); dispatch runs are gated by the endpoint's
    /// policy instead.
    /// </summary>
    public string? RequiredPermission { get; init; }

    /// <summary>
    /// How a call is categorized in the decision audit trail, and the single declaration of whether
    /// this tool writes - see <see cref="IsWrite"/>.
    /// </summary>
    public AgentDecisionType DecisionType { get; init; } = AgentDecisionType.Query;

    /// <summary>
    /// Whether the dispatch agent's catalogue includes this tool. False by default so copilot-only
    /// tools (invoicing, payments) never surface in dispatch conversations. The copilot is unaffected.
    /// </summary>
    public bool DispatchAgent { get; init; }

    /// <summary>
    /// Write tools mutate state: every call becomes a Suggested decision awaiting dispatcher
    /// approval, never executed inline. Derived from <see cref="DecisionType"/> rather than declared
    /// separately, so the two cannot disagree - naming a tool's audit type *is* declaring it a write.
    /// </summary>
    public bool IsWrite => DecisionType != AgentDecisionType.Query;
}
