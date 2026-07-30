using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Infrastructure.AI.Llm.Contracts;

namespace Logistics.Infrastructure.AI.Agents.Copilot;

/// <summary>
/// Serializes provider content blocks to the transcript's ContentJson and back. Tool_use ids must
/// round-trip exactly or the API rejects the replayed transcript. Unknown block types are skipped
/// on decode so old rows survive future block additions.
/// </summary>
internal static class CopilotTranscriptCodec
{
    public static string Encode(IReadOnlyList<LlmContentBlock> blocks)
    {
        var array = new JsonArray();
        foreach (var block in blocks)
        {
            JsonNode? node = block switch
            {
                LlmTextBlock text => new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = text.Text
                },
                LlmToolUseBlock toolUse => new JsonObject
                {
                    ["type"] = "tool_use",
                    ["id"] = toolUse.Id,
                    ["name"] = toolUse.Name,
                    ["input"] = toolUse.Input?.DeepClone()
                },
                LlmToolResultBlock toolResult => new JsonObject
                {
                    ["type"] = "tool_result",
                    ["tool_use_id"] = toolResult.ToolUseId,
                    ["content"] = toolResult.Content
                },
                _ => null
            };

            if (node is not null)
                array.Add(node);
        }

        return array.ToJsonString();
    }

    public static List<LlmContentBlock> Decode(string contentJson)
    {
        var blocks = new List<LlmContentBlock>();

        JsonArray? array;
        try
        {
            array = JsonNode.Parse(contentJson) as JsonArray;
        }
        catch (JsonException)
        {
            return blocks;
        }

        foreach (var node in array ?? [])
        {
            if (node is not JsonObject obj)
                continue;

            LlmContentBlock? block = obj["type"]?.GetValue<string>() switch
            {
                "text" => new LlmTextBlock(obj["text"]?.GetValue<string>() ?? ""),
                "tool_use" => new LlmToolUseBlock(
                    obj["id"]?.GetValue<string>() ?? "",
                    obj["name"]?.GetValue<string>() ?? "",
                    obj["input"]?.DeepClone()),
                "tool_result" => new LlmToolResultBlock(
                    obj["tool_use_id"]?.GetValue<string>() ?? "",
                    obj["content"]?.GetValue<string>() ?? ""),
                _ => null
            };

            if (block is not null)
                blocks.Add(block);
        }

        return blocks;
    }
}
