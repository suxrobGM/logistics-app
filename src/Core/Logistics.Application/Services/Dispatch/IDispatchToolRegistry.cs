namespace Logistics.Application.Services;

/// <summary>
/// Registry of all tools available to the dispatch agent.
/// Tool definitions are declared once and reused by both Claude API and future MCP server.
/// </summary>
public interface IDispatchToolRegistry
{
    IReadOnlyList<DispatchToolDefinition> GetToolDefinitions();
}

/// <summary>
/// Defines a single tool that the dispatch agent can use.
/// Compatible with both Claude API tool schemas and MCP tool definitions.
/// </summary>
public record DispatchToolDefinition(
    string Name,
    string Description,
    object InputSchema);
