using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Services;
using Logistics.Infrastructure.AI.Tools;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class DispatchToolExecutor(
    IEnumerable<IDispatchTool> tools,
    ILogger<DispatchToolExecutor> logger) : IDispatchToolExecutor
{
    private readonly Dictionary<string, IDispatchTool> toolMap = tools.ToDictionary(t => t.Name);

    public async Task<string> ExecuteToolAsync(string toolName, string toolInputJson, CancellationToken ct = default)
    {
        logger.LogInformation("Executing tool {ToolName}", toolName);
        var input = JsonNode.Parse(toolInputJson) ?? new JsonObject();

        if (!toolMap.TryGetValue(toolName, out var tool))
        {
            logger.LogWarning("Unknown tool requested: {ToolName}", toolName);
            return JsonSerializer.Serialize(new { error = $"Unknown tool: {toolName}" });
        }

        var result = await tool.ExecuteAsync(input, ct);
        logger.LogDebug("Tool {ToolName} returned {ResultLength} chars", toolName, result.Length);
        return result;
    }
}
