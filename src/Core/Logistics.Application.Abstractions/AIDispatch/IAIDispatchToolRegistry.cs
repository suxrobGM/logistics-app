using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
/// Registry of all tools available to the dispatch agent.
/// Tool definitions are declared once and reused by both Claude API and future MCP server.
/// </summary>
public interface IAIDispatchToolRegistry
{
    /// <summary>
    /// The tools a tenant with <paramref name="enabledFeatures"/> may call. A gated tool is dropped
    /// when its feature is off - its schema would otherwise cost tokens on every request.
    /// </summary>
    IReadOnlyList<AIDispatchToolDefinition> GetToolDefinitions(IReadOnlySet<TenantFeature> enabledFeatures);

    /// <summary>Every tool, gated or not. For surfaces that enforce features per call rather than per catalogue.</summary>
    IReadOnlyList<AIDispatchToolDefinition> GetAllToolDefinitions();
}

/// <summary>
/// Defines a single tool that the dispatch agent can use.
/// Compatible with both Claude API tool schemas and MCP tool definitions.
/// </summary>
/// <param name="RequiredFeature">
/// Gates the tool. Declared here so the schema filter and the MCP call-time check read the same
/// value - a new gated group is one field, not a new flag threaded through every caller.
/// </param>
public record AIDispatchToolDefinition(
    string Name,
    string Description,
    object InputSchema,
    TenantFeature? RequiredFeature = null);
