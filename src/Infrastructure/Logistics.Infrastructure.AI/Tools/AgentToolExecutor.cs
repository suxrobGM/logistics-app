using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.Agents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class AgentToolExecutor(
    IServiceProvider services,
    ILogger<AgentToolExecutor> logger) : IAgentToolExecutor
{
    public async Task<string> ExecuteToolAsync(string toolName, string toolInputJson, CancellationToken ct = default)
    {
        logger.LogInformation("Executing tool {ToolName}", toolName);

        if (AgentToolCatalog.ImplementationFor(toolName) is not { } toolType)
        {
            logger.LogWarning("Unknown tool requested: {ToolName}", toolName);
            return ToolResult.Error($"Unknown tool: {toolName}");
        }

        JsonNode input;
        try
        {
            input = JsonNode.Parse(toolInputJson) ?? new JsonObject();
        }
        catch (JsonException)
        {
            return ToolResult.Error("Tool arguments were not valid JSON.");
        }

        // Resolved by name: injecting IEnumerable<IAgentTool> built every tool, and everything
        // behind it, on each turn to answer one call.
        var tool = (IAgentTool)services.GetRequiredService(toolType);

        var result = await tool.ExecuteAsync(input, ct);
        logger.LogDebug("Tool {ToolName} returned {ResultLength} chars", toolName, result.Length);
        return result;
    }
}
