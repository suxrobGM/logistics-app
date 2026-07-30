using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.Agents;

/// <summary>
/// Registry of all tools available to the AI agents (dispatch and copilot).
/// Tool definitions are declared once and reused by the agent loop and the MCP server.
/// </summary>
/// <remarks>
/// One member per surface rather than one filtered query with flags: the permission set is then
/// required exactly where it applies, so a copilot catalogue cannot be built unscoped by accident.
/// </remarks>
public interface IAgentToolRegistry
{
    /// <summary>
    /// The catalogue for a fleet dispatch run: only tools that opt in via
    /// <see cref="AgentToolDefinition.DispatchAgent"/>, because this agent can execute autonomously.
    /// Tools whose feature is off are dropped - their schemas cost tokens on every request.
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

    /// <summary>Every tool, gated or not. For surfaces that gate per call rather than per catalogue.</summary>
    IReadOnlyList<AgentToolDefinition> GetAllTools();

    /// <summary>Null for unknown (e.g. hallucinated) tool names.</summary>
    AgentToolDefinition? TryGetDefinition(string name);
}
