using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>Filters <see cref="AgentToolCatalog"/> down to what each surface may see.</summary>
internal sealed class AgentToolRegistry : IAgentToolRegistry
{
    /// <summary>True on the agent surfaces only - MCP executes writes for real.</summary>
    private const string ApprovalNote =
        " Creates a suggestion for dispatcher approval - it is not executed immediately.";

    public IReadOnlyList<AgentToolDefinition> GetDispatchAgentTools(
        IReadOnlySet<TenantFeature> enabledFeatures) =>
        [.. Available(enabledFeatures).Where(t => t.DispatchAgent).Select(ForAgent)];

    public IReadOnlyList<AgentToolDefinition> GetCopilotTools(
        IReadOnlySet<TenantFeature> enabledFeatures,
        IReadOnlySet<string> callerPermissions) =>
        [.. Available(enabledFeatures)
            .Where(t => t.RequiredPermission is null || callerPermissions.Contains(t.RequiredPermission))
            .Select(ForAgent)];

    public IReadOnlyList<AgentToolDefinition> GetMcpTools(IReadOnlySet<TenantFeature> enabledFeatures) =>
        [.. Available(enabledFeatures).Where(t => !t.RequiresHumanOrigin)];

    public IReadOnlyList<AgentToolDefinition> GetAllTools() => AgentToolCatalog.Definitions;

    public AgentToolDefinition? TryGetDefinition(string name) => AgentToolCatalog.DefinitionFor(name);

    private static IEnumerable<AgentToolDefinition> Available(IReadOnlySet<TenantFeature> enabled) =>
        AgentToolCatalog.Definitions
            .Where(t => t.RequiredFeature is null || enabled.Contains(t.RequiredFeature.Value));

    private static AgentToolDefinition ForAgent(AgentToolDefinition definition) =>
        definition.IsWrite
            ? definition with { Description = definition.Description + ApprovalNote }
            : definition;
}
