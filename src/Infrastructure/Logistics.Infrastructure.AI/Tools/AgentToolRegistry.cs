using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>Filters <see cref="AgentToolCatalog"/> down to what each surface may see.</summary>
internal sealed class AgentToolRegistry : IAgentToolRegistry
{
    /// <summary>True on the agent surfaces only - MCP executes writes for real.</summary>
    private const string ApprovalNote =
        " Creates a suggestion for dispatcher approval - it is not executed immediately.";

    /// <summary>The MCP counterpart: there is no approval step behind an API key.</summary>
    private const string McpWriteWarning =
        " WRITE OPERATION: this takes effect immediately. Explain what you are about to do and get explicit user confirmation before calling it.";

    public IReadOnlyList<AgentToolDefinition> GetDispatchAgentTools(
        IReadOnlySet<TenantFeature> enabledFeatures) =>
        [.. On(AgentSurfaces.Dispatch, enabledFeatures).Select(ForAgent)];

    public IReadOnlyList<AgentToolDefinition> GetCopilotTools(
        IReadOnlySet<TenantFeature> enabledFeatures,
        IReadOnlySet<string> callerPermissions) =>
        [.. On(AgentSurfaces.Copilot, enabledFeatures)
            .Where(t => t.RequiredPermission is null || callerPermissions.Contains(t.RequiredPermission))
            .Select(ForAgent)];

    /// <summary>
    /// Built from <see cref="McpDenialReason"/> rather than repeating its rules, so what the
    /// catalogue hides and what a call is refused for cannot come apart.
    /// </summary>
    public IReadOnlyList<AgentToolDefinition> GetMcpTools(IReadOnlySet<TenantFeature> enabledFeatures) =>
        [.. AgentToolCatalog.Definitions
            .Where(t => McpDenialReason(t.Name, enabledFeatures) is null)
            .Select(ForMcp)];

    public string? McpDenialReason(string toolName, IReadOnlySet<TenantFeature> enabledFeatures)
    {
        if (AgentToolCatalog.DefinitionFor(toolName) is not { } definition)
            return $"Unknown tool: {toolName}";

        if (!definition.Surfaces.HasFlag(AgentSurfaces.Mcp))
        {
            return $"The {toolName} tool is not available over MCP: it needs either a person to "
                + "attribute the action to or a dispatcher to approve it, and an API key provides "
                + "neither. Ask the user to run it from the app.";
        }

        return definition.RequiredFeature is { } feature && !enabledFeatures.Contains(feature)
            ? $"The {feature.GetDescription()} feature is not enabled for this tenant."
            : null;
    }

    public AgentToolDefinition? TryGetDefinition(string name) => AgentToolCatalog.DefinitionFor(name);

    private static IEnumerable<AgentToolDefinition> On(
        AgentSurfaces surface,
        IReadOnlySet<TenantFeature> enabled) =>
        AgentToolCatalog.Definitions
            .Where(t => t.Surfaces.HasFlag(surface))
            .Where(t => t.RequiredFeature is null || enabled.Contains(t.RequiredFeature.Value));

    private static AgentToolDefinition ForAgent(AgentToolDefinition definition) =>
        WithWriteNote(definition, ApprovalNote);

    private static AgentToolDefinition ForMcp(AgentToolDefinition definition) =>
        WithWriteNote(definition, McpWriteWarning);

    private static AgentToolDefinition WithWriteNote(AgentToolDefinition definition, string note) =>
        definition.IsWrite
            ? definition with { Description = definition.Description + note }
            : definition;
}
