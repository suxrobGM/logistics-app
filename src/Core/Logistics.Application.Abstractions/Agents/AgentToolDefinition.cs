using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.Agents;

/// <summary>
/// A single tool the AI agents can use. Compatible with both Claude API tool schemas and MCP.
/// </summary>
/// <remarks>
/// Only name and description are positional. Everything else is an init property so that every
/// piece of behaviour metadata is named at the declaration site - a bare <see cref="TenantFeature"/>
/// in the third slot is otherwise easy to mistake for something else.
/// </remarks>
public record AgentToolDefinition(string Name, string Description)
{
    /// <summary>
    /// The JSON Schema the model is shown, generated from the tool's input type so it cannot drift
    /// from what the tool reads. The default stands in for a tool that takes no arguments.
    /// </summary>
    public JsonNode InputSchema { get; init; } =
        new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() };

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
    /// Which catalogues publish this tool. Defaults to the copilot alone, so a tool nobody widened
    /// is under-exposed rather than over-exposed - the dispatch run and MCP both call writes with no
    /// individual behind them, in different ways, and each has to be asked for by name.
    /// </summary>
    public AgentSurfaces Surfaces { get; init; } = AgentSurfaces.Copilot;

    /// <summary>
    /// The tool can overwrite or undo something the caller did not name. MCP clients read this to
    /// decide whether a call may be auto-approved, so it belongs to the tool rather than being
    /// asserted once for the whole catalogue.
    /// </summary>
    public bool Destructive { get; init; }

    /// <summary>
    /// Write tools mutate state. On the agent surfaces every call becomes a Suggested decision
    /// awaiting approval; over MCP there is no approval step, which is why a write must name
    /// <see cref="AgentSurfaces.Mcp"/> to get there. Derived from <see cref="DecisionType"/> rather
    /// than declared separately, so the two cannot disagree - naming a tool's audit type *is*
    /// declaring it a write.
    /// </summary>
    public bool IsWrite => DecisionType != AgentDecisionType.Query;
}
