using System.Text.Json;
using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Llm.Providers;

/// <summary>Tool-call argument decoding shared by both OpenAI-shaped providers.</summary>
internal static class OpenAIToolArguments
{
    /// <summary>
    /// A call truncated at the output-token ceiling yields malformed JSON; null reaches the tool
    /// executor as an argument error rather than an opaque provider crash.
    /// </summary>
    public static JsonNode? Parse(BinaryData? arguments)
    {
        var json = arguments?.ToString();
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
