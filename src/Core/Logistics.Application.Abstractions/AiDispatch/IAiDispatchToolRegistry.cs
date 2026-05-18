using Logistics.Application.Abstractions.AiDispatch;
namespace Logistics.Application.Abstractions.AiDispatch;

/// <summary>
/// Registry of all tools available to the dispatch agent.
/// Tool definitions are declared once and reused by both Claude API and future MCP server.
/// </summary>
public interface IAiDispatchToolRegistry
{
    IReadOnlyList<AiDispatchToolDefinition> GetToolDefinitions(bool includeLoadBoardTools = false);
}

/// <summary>
/// Defines a single tool that the dispatch agent can use.
/// Compatible with both Claude API tool schemas and MCP tool definitions.
/// </summary>
public record AiDispatchToolDefinition(
    string Name,
    string Description,
    object InputSchema);
