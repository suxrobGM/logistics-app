using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenAI.Responses;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Application.Abstractions.AI;

// OPENAI001: the SDK marks the Responses surface [Experimental]; the wire protocol is stable.
#pragma warning disable OPENAI001

namespace Logistics.Infrastructure.AI.Llm.Providers;

/// <summary>
/// LLM provider for OpenAI's Responses API (<c>/v1/responses</c>) - the only surface where function
/// tools and a non-<c>none</c> reasoning effort coexist. Compatible endpoints don't serve this route
/// and stay on <see cref="OpenAICompatibleLlmProvider"/>.
/// </summary>
internal sealed class OpenAIResponsesLlmProvider(LlmProviderOptions config, HttpClient httpClient) : ILlmProvider
{
    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct)
    {
        // System.ClientModel has no Timeout of its own - the only way to bound a call is to hand it
        // a transport over our factory-pooled HttpClient, which carries the configured timeout.
        var clientOptions = new ResponsesClientOptions
        {
            Transport = new HttpClientPipelineTransport(httpClient)
        };

        if (config.BaseUrl is not null)
            clientOptions.Endpoint = new Uri(config.BaseUrl);

        var client = new ResponsesClient(new ApiKeyCredential(config.ApiKey), clientOptions);

        var options = BuildOptions(request);

        foreach (var tool in request.Tools)
        {
            options.Tools.Add(ResponseTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description,
                functionParameters: BinaryData.FromString(tool.InputSchema.ToJsonString()),
                // Hand-written schemas don't satisfy strict mode's additionalProperties:false rule.
                strictModeEnabled: false));
        }

        foreach (var item in BuildInputItems(request))
        {
            options.InputItems.Add(item);
        }

        var response = await client.CreateResponseAsync(options, ct);
        return MapResponse(response.Value);
    }

    /// <summary>
    /// Builds the request options. The system prompt goes in as an input item, not here - see
    /// <see cref="BuildInputItems"/>.
    /// </summary>
    internal static CreateResponseOptions BuildOptions(LlmRequest request)
    {
        var options = new CreateResponseOptions
        {
            Model = request.Model,
            MaxOutputTokenCount = request.MaxTokens,
            Temperature = request.Temperature.HasValue ? (float)request.Temperature.Value : null,

            // Responses defaults to store:true; tool outputs carry driver names and HOS data.
            StoredOutputEnabled = false
        };

        if (LlmModelCatalog.ReasoningStyleOf(request.Model) == ReasoningStyle.OpenAIEffort)
        {
            options.ReasoningOptions = new ResponseReasoningOptions
            {
                ReasoningEffortLevel = request.Effort switch
                {
                    ReasoningEffort.None => ResponseReasoningEffortLevel.None,
                    ReasoningEffort.Low => ResponseReasoningEffortLevel.Low,
                    ReasoningEffort.Medium => ResponseReasoningEffortLevel.Medium,
                    // The SDK's effort scale tops out at High - XHigh and Max both clamp to it.
                    _ => ResponseReasoningEffortLevel.High
                }
            };
        }

        return options;
    }

    /// <summary>Flattens the system prompt and transcript into one ordered input-item list.</summary>
    internal static IEnumerable<ResponseItem> BuildInputItems(LlmRequest request)
    {
        yield return ResponseItem.CreateDeveloperMessageItem(request.SystemPrompt);

        foreach (var item in request.Messages.SelectMany(ToInputItems))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Maps one provider-agnostic message to one or more input items. Responses has no tool role:
    /// a call and its result are each a <i>top-level</i> item keyed by <c>call_id</c>, so an
    /// assistant turn chat completions collapsed into one message fans out into 1+N items.
    /// </summary>
    internal static IEnumerable<ResponseItem> ToInputItems(LlmMessage message)
    {
        if (message.Role == LlmRole.User)
        {
            var toolResults = message.Content.OfType<LlmToolResultBlock>().ToList();
            if (toolResults.Count > 0)
            {
                return toolResults.Select(ResponseItem (result) =>
                    ResponseItem.CreateFunctionCallOutputItem(
                        callId: result.ToolUseId,
                        functionOutput: result.Content));
            }

            var textParts = message.Content.OfType<LlmTextBlock>().ToList();
            var text = string.Join("\n", textParts.Select(t => t.Text));
            var documents = message.Content.OfType<LlmDocumentBlock>().ToList();

            if (documents.Count == 0)
                return [ResponseItem.CreateUserMessageItem(text)];

            var parts = new List<ResponseContentPart>();
            if (!string.IsNullOrEmpty(text))
            {
                parts.Add(ResponseContentPart.CreateInputTextPart(text));
            }

            foreach (var document in documents)
            {
                parts.Add(ResponseContentPart.CreateInputFilePart(
                    fileBytes: BinaryData.FromBytes(Convert.FromBase64String(document.Base64Data)),
                    fileBytesMediaType: document.MediaType,
                    filename: "document.pdf"));
            }

            return [ResponseItem.CreateUserMessageItem(parts)];
        }

        return ToAssistantItems(message);
    }

    private static IEnumerable<ResponseItem> ToAssistantItems(LlmMessage message)
    {
        // Text may be interleaved with tool calls (Anthropic's shape); collapse it ahead of them.
        var textParts = message.Content.OfType<LlmTextBlock>().Select(t => t.Text).ToList();
        var text = textParts.Count > 0 ? string.Join("\n\n", textParts) : null;

        // An empty output_text part is a 400 here, where chat completions tolerated it.
        if (!string.IsNullOrEmpty(text))
        {
            yield return ResponseItem.CreateAssistantMessageItem(text);
        }

        // LlmThinkingBlock is not replayed: Responses accepts a missing reasoning item but rejects
        // one not immediately followed by its function_call. The model re-reasons each iteration.
        foreach (var toolUse in message.Content.OfType<LlmToolUseBlock>())
        {
            yield return ResponseItem.CreateFunctionCallItem(
                callId: toolUse.Id,
                functionName: toolUse.Name,
                functionArguments: BinaryData.FromString(toolUse.Input?.ToJsonString() ?? "{}"));
        }
    }

    private static LlmResponse MapResponse(ResponseResult response)
    {
        var content = new List<LlmContentBlock>();
        var textParts = new List<string>();
        var toolCalls = new List<LlmToolUseBlock>();

        foreach (var item in response.OutputItems)
        {
            switch (item)
            {
                case MessageResponseItem message:
                    foreach (var part in message.Content)
                    {
                        if (string.IsNullOrEmpty(part.Text))
                            continue;

                        textParts.Add(part.Text);
                        content.Add(new LlmTextBlock(part.Text));
                    }
                    break;

                // CallId (call_...), never Id (fc_...): the tool result must answer CallId, and the
                // wrong one is an unmatched-call 400 that also persists into stored conversations.
                case FunctionCallResponseItem call:
                    var block = new LlmToolUseBlock(
                        call.CallId,
                        call.FunctionName,
                        OpenAIToolArguments.Parse(call.FunctionArguments));
                    content.Add(block);
                    toolCalls.Add(block);
                    break;
            }
        }

        var textContent = textParts.Count > 0 ? string.Join("\n\n", textParts) : null;

        return new LlmResponse
        {
            AssistantMessage = new LlmMessage(LlmRole.Assistant, content),
            TextContent = textContent,
            // From the output items, not Status: Status is Completed both when the model answered
            // and when it asked for tools, which would end every tool session after one iteration.
            StopReason = toolCalls.Count > 0 ? "tool_use" : "end_turn",
            ToolCalls = toolCalls,
            Usage = OpenAIUsage.From(
                response.Usage?.InputTokenCount ?? 0,
                response.Usage?.InputTokenDetails?.CachedTokenCount ?? 0,
                response.Usage?.OutputTokenCount ?? 0)
        };
    }
}
