using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Application.Abstractions.AI;

/// <summary>
/// Reads a tool's JSON output back. A write tool that fails returns <c>success: false</c> with an
/// <c>error</c> rather than throwing, so a caller that only catches exceptions records it as executed.
/// </summary>
public static class ToolOutput
{
    public static JsonObject? Parse(string output)
    {
        try
        {
            return JsonNode.Parse(output) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool IsFailure(JsonObject? output) =>
        output?["success"]?.GetValueKind() == JsonValueKind.False;
}
