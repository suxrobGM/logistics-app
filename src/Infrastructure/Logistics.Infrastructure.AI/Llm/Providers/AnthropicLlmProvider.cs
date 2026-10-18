using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic;
using Anthropic.Core;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;

namespace Logistics.Infrastructure.AI.Llm.Providers;

internal sealed class AnthropicLlmProvider(LlmProviderOptions config, HttpClient httpClient) : ILlmProvider
{
    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct)
    {
        // The SDK retries 408/409/429/5xx itself, hence no resilience handler on the pooled client.
        var client = new AnthropicClient
        {
            ApiKey = config.ApiKey,
            HttpClient = httpClient,
            Timeout = httpClient.Timeout
        };

        try
        {
            var response = await client.Messages.Create(BuildParameters(request), ct);
            return MapResponse(response);
        }
        catch (AnthropicApiException ex)
        {
            throw new LlmProviderException(
                LlmProviderException.KindFor((int)ex.StatusCode), ex.Message, ex);
        }
        catch (AnthropicIOException ex)
        {
            throw new LlmProviderException(LlmFailureKind.Unavailable, ex.Message, ex);
        }
    }

    /// <summary>
    /// Adaptive models reject a temperature and have no clean "off" - omitting <c>thinking</c>
    /// still runs it at the default effort - so they always get an explicit effort, and
    /// <see cref="ReasoningEffort.None"/> maps to <c>low</c>.
    /// </summary>
    internal static MessageCreateParams BuildParameters(LlmRequest request)
    {
        var isAdaptive = LlmModelCatalog.ReasoningStyleOf(request.Model) == ReasoningStyle.AnthropicAdaptive;

        // The SDK sends an assigned null as JSON null, so optional fields are only ever set to a value.
        var parameters = new MessageCreateParams
        {
            Model = request.Model,
            MaxTokens = request.MaxTokens,
            System = new List<TextBlockParam>
            {
                new() { Text = request.SystemPrompt, CacheControl = new CacheControlEphemeral() }
            },
            Messages = BuildMessages(request.Messages),
            Tools = BuildTools(request)
        };

        if (isAdaptive)
        {
            return parameters with
            {
                Thinking = new ThinkingConfigAdaptive(),
                OutputConfig = new OutputConfig { Effort = EffortFor(request.Effort) }
            };
        }

        return request.Temperature is { } temperature
            ? parameters with { Temperature = (double)temperature }
            : parameters;
    }

    internal static Effort EffortFor(ReasoningEffort effort) => effort switch
    {
        ReasoningEffort.None or ReasoningEffort.Low => Effort.Low,
        ReasoningEffort.Medium => Effort.Medium,
        ReasoningEffort.XHigh => Effort.Xhigh,
        ReasoningEffort.Max => Effort.Max,
        _ => Effort.High
    };

    /// <summary>Web search runs server-side inside the call; <c>max_uses</c> bounds its spend.</summary>
    internal static List<ToolUnion> BuildTools(LlmRequest request)
    {
        var tools = request.Tools
            .Select(t => new ToolUnion(new Tool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = ToInputSchema(t.InputSchema)
            }))
            .ToList();

        tools.Add(WebSearchFor(request.Model));
        return tools;
    }

    /// <summary>Of the catalog's models only Sonnet accepts the dynamic-filtering variant.</summary>
    private static ToolUnion WebSearchFor(string model) =>
        model.StartsWith("claude-sonnet", StringComparison.Ordinal)
            ? new ToolUnion(new WebSearchTool20260209 { MaxUses = 3 })
            : new ToolUnion(new WebSearchTool20250305 { MaxUses = 3 });

    private static InputSchema ToInputSchema(JsonNode schema)
    {
        var properties = ToInputDictionary(schema["properties"]);
        var required = schema["required"] is JsonArray req
            ? req.Select(r => r!.GetValue<string>()).ToList()
            : [];

        return new InputSchema { Properties = properties, Required = required };
    }

    /// <summary>
    /// Keeps every block, in order: the turn must replay exactly as it arrived, server-tool blocks
    /// included.
    /// </summary>
    internal static LlmResponse MapResponse(Message response)
    {
        var content = new List<LlmContentBlock>();
        foreach (var block in response.Content)
        {
            if (block.TryPickText(out var text))
                content.Add(new LlmTextBlock(text.Text));
            else if (block.TryPickToolUse(out var toolUse))
                content.Add(new LlmToolUseBlock(toolUse.ID, toolUse.Name, JsonSerializer.SerializeToNode(toolUse.Input)));
            else if (block.TryPickThinking(out var thinking))
                content.Add(new LlmThinkingBlock(thinking.Thinking, thinking.Signature));
            // Redacted blocks replay too - dropping one leaves a hole the API rejects next iteration.
            else if (block.TryPickRedactedThinking(out var redacted))
                content.Add(new LlmRedactedThinkingBlock(redacted.Data));
            else
                content.Add(new LlmProviderBlock(JsonSerializer.Serialize(block)));
        }

        var textBlocks = content.OfType<LlmTextBlock>().Select(t => t.Text).ToList();

        return new LlmResponse
        {
            AssistantMessage = new LlmMessage(LlmRole.Assistant, content),
            TextContent = textBlocks.Count > 0 ? string.Join("\n\n", textBlocks) : null,
            StopReason = MapStopReason(response.StopReason),
            ToolCalls = [.. content.OfType<LlmToolUseBlock>()],
            Usage = new LlmTokenUsage(
                (int)response.Usage.InputTokens,
                (int)response.Usage.OutputTokens,
                (int)(response.Usage.CacheReadInputTokens ?? 0),
                (int)(response.Usage.CacheCreationInputTokens ?? 0))
        };
    }

    private static LlmStopReason MapStopReason(ApiEnum<string, StopReason>? stopReason) =>
        stopReason?.Value() switch
        {
            StopReason.ToolUse => LlmStopReason.ToolUse,
            StopReason.MaxTokens or StopReason.ModelContextWindowExceeded => LlmStopReason.MaxTokens,
            StopReason.PauseTurn => LlmStopReason.PauseTurn,
            StopReason.Refusal => LlmStopReason.Refusal,
            _ => LlmStopReason.EndTurn
        };

    /// <summary>
    /// One cache breakpoint, on the newest message's last block: a breakpoint looks back only 20
    /// blocks, and caches only up to itself.
    /// </summary>
    internal static List<MessageParam> BuildMessages(IReadOnlyList<LlmMessage> messages)
    {
        var mapped = new List<MessageParam>(messages.Count);
        for (var i = 0; i < messages.Count; i++)
        {
            mapped.Add(ToMessageParam(messages[i], cacheLastBlock: i == messages.Count - 1));
        }

        return mapped;
    }

    internal static MessageParam ToMessageParam(LlmMessage message, bool cacheLastBlock = false)
    {
        var blocks = message.Content.Select(ToBlockParam).ToList();

        // The newest message is a user turn, so its last block is never a thinking block.
        if (cacheLastBlock && blocks.Count > 0)
            blocks[^1] = CachedBlockParam(message.Content[^1]);

        return new MessageParam
        {
            Role = message.Role == LlmRole.User ? Role.User : Role.Assistant,
            Content = blocks
        };
    }

    private static ContentBlockParam ToBlockParam(LlmContentBlock block) => block switch
    {
        LlmTextBlock text => new TextBlockParam { Text = text.Text },
        LlmToolUseBlock toolUse => new ToolUseBlockParam
        {
            ID = toolUse.Id,
            Name = toolUse.Name,
            Input = ToInputDictionary(toolUse.Input)
        },
        LlmToolResultBlock result => new ToolResultBlockParam { ToolUseID = result.ToolUseId, Content = result.Content },
        LlmDocumentBlock document => new DocumentBlockParam { Source = new Base64PdfSource { Data = document.Base64Data } },
        LlmThinkingBlock thinking => new ThinkingBlockParam
        {
            Thinking = thinking.Thinking,
            Signature = thinking.Signature ?? ""
        },
        LlmRedactedThinkingBlock redacted => new RedactedThinkingBlockParam { Data = redacted.Data },
        LlmProviderBlock native => JsonSerializer.Deserialize<ContentBlockParam>(native.Json)
            ?? throw new InvalidOperationException("Provider block did not deserialize."),
        _ => throw new NotSupportedException($"Unsupported content block {block.GetType().Name}.")
    };

    private static ContentBlockParam CachedBlockParam(LlmContentBlock block) => block switch
    {
        LlmTextBlock text => new TextBlockParam { Text = text.Text, CacheControl = new CacheControlEphemeral() },
        LlmToolResultBlock result => new ToolResultBlockParam
        {
            ToolUseID = result.ToolUseId,
            Content = result.Content,
            CacheControl = new CacheControlEphemeral()
        },
        LlmDocumentBlock document => new DocumentBlockParam
        {
            Source = new Base64PdfSource { Data = document.Base64Data },
            CacheControl = new CacheControlEphemeral()
        },
        _ => ToBlockParam(block)
    };

    private static Dictionary<string, JsonElement> ToInputDictionary(JsonNode? input) =>
        input is JsonObject obj
            ? obj.ToDictionary(p => p.Key, p => JsonSerializer.SerializeToElement(p.Value))
            : [];
}
