using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.Agents;

/// <summary>
/// Registry of all tools available to the AI agents (dispatch and copilot).
/// Tool definitions are declared once, next to the tool, and reused by every surface.
/// </summary>
/// <remarks>
/// One member per surface rather than one filtered query with flags: the permission set is then
/// required exactly where it applies, so a copilot catalogue cannot be built unscoped by accident.
/// </remarks>
public interface IAgentToolRegistry
{
    /// <summary>
    /// The catalogue for a fleet dispatch run: only tools that opt in via
    /// <see cref="AgentToolDefinition.DispatchAgent"/>. Tools whose feature is off are dropped -
    /// their schemas cost tokens on every request.
    /// </summary>
    IReadOnlyList<AgentToolDefinition> GetDispatchAgentTools(IReadOnlySet<TenantFeature> enabledFeatures);

    /// <summary>
    /// The catalogue for one copilot turn, scoped to the calling user: any tool whose
    /// <see cref="AgentToolDefinition.RequiredPermission"/> the caller lacks is dropped, so the model
    /// never sees a tool it cannot use.
    /// </summary>
    IReadOnlyList<AgentToolDefinition> GetCopilotTools(
        IReadOnlySet<TenantFeature> enabledFeatures,
        IReadOnlySet<string> callerPermissions);

    /// <summary>
    /// The catalogue an MCP client is shown: filtered on features, minus the tools that need a
    /// human origin. Permissions do not apply - an API key authenticates a tenant, not a person.
    /// </summary>
    IReadOnlyList<AgentToolDefinition> GetMcpTools(IReadOnlySet<TenantFeature> enabledFeatures);

    /// <summary>Every tool, gated or not, with no surface-specific wording applied.</summary>
    IReadOnlyList<AgentToolDefinition> GetAllTools();

    /// <summary>Null for unknown (e.g. hallucinated) tool names.</summary>
    AgentToolDefinition? TryGetDefinition(string name);
}
