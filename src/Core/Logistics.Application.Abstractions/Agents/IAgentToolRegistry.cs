using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.Agents;

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
    IReadOnlyList<AgentToolDefinition> GetToolDefinitions(
        IReadOnlySet<TenantFeature> enabledFeatures,
        IReadOnlySet<string>? callerPermissions = null,
        bool forDispatchAgent = false);

    /// <summary>Every tool, gated or not. For surfaces that gate per call rather than per catalogue.</summary>
    IReadOnlyList<AgentToolDefinition> GetAllToolDefinitions();

    /// <summary>Null for unknown (e.g. hallucinated) tool names.</summary>
    AgentToolDefinition? TryGetDefinition(string name);
}
