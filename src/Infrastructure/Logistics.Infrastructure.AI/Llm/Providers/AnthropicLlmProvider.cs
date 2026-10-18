using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic;
using Anthropic.Core;
using Anthropic.Core;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;

namespace Logistics.Infrastructure.AI.Llm.Providers;

/// <summary>
/// LLM provider over the official Anthropic SDK (<c>Anthropic</c> package).
/// </summary>
internal sealed class AnthropicLlmProvider(LlmProviderOptions config, HttpClient httpClient) : ILlmProvider
{
    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct)
    {
        // The HttpClient is factory-pooled; the SDK keeps its own retries (408/409/429/5xx), which
        // is why the named client carries no resilience handler.
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
    /// The full request. Adaptive models never get a temperature (they reject non-default
    /// sampling) and always get adaptive thinking plus an explicit effort: neither Sonnet 5.5 nor
    /// Haiku 5.5 has a clean "off", and omitting <c>thinking</c> still runs adaptive at the
    /// model's default effort - so <see cref="ReasoningEffort.None"/> rides on <c>low</c>.
    /// </summary>
    internal static MessageCreateParams BuildParameters(LlmRequest request)
    {
        var isAdaptive = LlmModelCatalog.ReasoningStyleOf(request.Model) == ReasoningStyle.AnthropicAdaptive;

        // Optional fields are set only when they carry a value: the SDK serializes an assigned null as
        // a JSON null rather than leaving the field out.
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

    /// <summary>
    /// Function tools plus Anthropic's server-side web search. The search runs inside one API
    /// call, so the agent loop never executes it; <c>max_uses</c> bounds the per-call spend.
    /// </summary>
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

    /// <summary>
    /// The dynamic-filtering web search is Sonnet-only among the catalog's models; Haiku gets the
    /// basic variant, which every model accepts.
    /// </summary>
    private static ToolUnion WebSearchFor(string model) =>
        model.StartsWith("claude-sonnet", StringComparison.Ordinal)
            ? new ToolUnion(new WebSearchTool20260209 { MaxUses = 3 })
            : new ToolUnion(new WebSearchTool20250305 { MaxUses = 3 });

    private static InputSchema ToInputSchema(JsonNode schema)
    {
        var properties = schema["properties"] is JsonObject props
            ? props.ToDictionary(p => p.Key, p => JsonSerializer.SerializeToElement(p.Value))
            : [];

        var required = schema["required"] is JsonArray req
            ? req.Select(r => r!.GetValue<string>()).ToList()
            : [];

        return new InputSchema { Properties = properties, Required = required };
    }

    /// <summary>
    /// Preserves block order and keeps every text block: replies interleave text with tool calls,
    /// and the transcript must replay in the shape it arrived. Server-tool blocks are kept verbatim
    /// so the turn replays exactly as the model produced it.
    /// </summary>
    internal static LlmResponse MapResponse(Message response)
    {
        var content = new List<LlmContentBlock>();
        foreach (var block in response.Content)
        {
            if (block.TryPickText(out var text))
                content.Add(new LlmTextBlock(text.Text));
            else if (block.TryPickToolUse(out var toolUse))
                content.Add(new LlmToolUseBlock(toolUse.ID, toolUse.Name, ToJsonNode(toolUse.Input)));
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
    /// Maps the transcript and caches its prefix - the loop resends the whole conversation every
    /// iteration, and a cache read costs a tenth of full input.
    /// </summary>
    /// <remarks>
    /// Breakpoint placement is load-bearing: newest message because a breakpoint only searches back
    /// 20 content blocks, last block because the prefix caches only up to it.
    /// </remarks>
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

        // The newest message is always a user turn (the prompt, or tool results), so its last
        // block is text, a tool result or a document - never a thinking block.
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

    /// <summary>
    /// The cache breakpoint's block. Built separately because the SDK sends an assigned null
    /// <c>cache_control</c> as a JSON null, so only this block may set it.
    /// </summary>
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

    private static JsonNode? ToJsonNode(IReadOnlyDictionary<string, JsonElement> input) =>
        JsonSerializer.SerializeToNode(input);

    private static Dictionary<string, JsonElement> ToInputDictionary(JsonNode? input) =>
        input is JsonObject obj
            ? obj.ToDictionary(p => p.Key, p => JsonSerializer.SerializeToElement(p.Value))
            : [];
}
