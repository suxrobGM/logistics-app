using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>Failed writes carry <c>success: false</c>; create tools name what they made (<c>trip_id</c>).</summary>
internal static class ToolOutputReader
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
