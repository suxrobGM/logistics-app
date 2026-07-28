using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
/// Registry of all tools available to the AI agents (dispatch and copilot).
/// Tool definitions are declared once and reused by the agent loop and the MCP server.
/// </summary>
public interface IAIDispatchToolRegistry
{
    /// <summary>
    /// The tools a tenant with <paramref name="enabledFeatures"/> may call. A gated tool is dropped
    /// when its feature is off - its schema would otherwise cost tokens on every request.
    /// When <paramref name="callerPermissions"/> is provided, tools whose
    /// <see cref="AIDispatchToolDefinition.RequiredPermission"/> the caller lacks are dropped too,
    /// so the model never sees a tool it cannot use.
    /// </summary>
    IReadOnlyList<AIDispatchToolDefinition> GetToolDefinitions(
        IReadOnlySet<TenantFeature> enabledFeatures,
        IReadOnlySet<string>? callerPermissions = null);

    /// <summary>Every tool, gated or not. For surfaces that enforce features per call rather than per catalogue.</summary>
    IReadOnlyList<AIDispatchToolDefinition> GetAllToolDefinitions();

    /// <summary>The definition for <paramref name="name"/>, or null for unknown (e.g. hallucinated) tool names.</summary>
    AIDispatchToolDefinition? TryGetDefinition(string name);
}

/// <summary>
/// Defines a single tool that the AI agents can use.
/// Compatible with both Claude API tool schemas and MCP tool definitions.
/// </summary>
/// <param name="RequiredFeature">
/// Gates the tool. Declared here so the schema filter and the MCP call-time check read the same
/// value - a new gated group is one field, not a new flag threaded through every caller.
/// </param>
/// <param name="IsWrite">
/// Write tools mutate state: HumanInTheLoop turns them into Suggested decisions awaiting approval,
/// Autonomous executes them. This flag is the single registration point - there is no separate
/// write-tool list anywhere else.
/// </param>
/// <param name="RequiredPermission">
/// The caller permission the tool maps to. Enforced only for permission-scoped runs (copilot);
/// dispatch runs pass no caller permissions and are gated by the endpoint's policy instead.
/// </param>
/// <param name="DecisionType">How a call to this tool is categorized in the decision audit trail.</param>
public record AIDispatchToolDefinition(
    string Name,
    string Description,
    object InputSchema,
    TenantFeature? RequiredFeature = null,
    bool IsWrite = false,
    string? RequiredPermission = null,
    AIDispatchDecisionType DecisionType = AIDispatchDecisionType.Query);
