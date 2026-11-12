using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Reads the JSON a write tool returns: <c>ToolResult.WriteFailed</c> marks failure with
/// <c>success: false</c>, and create tools name what they made (<c>trip_id</c>, <c>invoice_id</c>).
/// </summary>
internal static class ToolOutputReader
{
    public static bool IsFailure(string output) =>
        Parse(output) is JsonObject obj
        && obj["success"] is JsonValue success
        && success.TryGetValue<bool>(out var ok)
        && !ok;

    public static Guid? GetGuid(string output, string key) =>
        Parse(output) is JsonObject obj
        && obj[key] is JsonValue value
        && value.TryGetValue<string>(out var text)
        && Guid.TryParse(text, out var id)
            ? id
            : null;

    private static JsonNode? Parse(string output)
    {
        try
        {
            return JsonNode.Parse(output);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
