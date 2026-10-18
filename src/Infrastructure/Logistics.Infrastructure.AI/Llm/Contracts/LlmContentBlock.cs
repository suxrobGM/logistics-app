using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Llm.Contracts;

internal abstract record LlmContentBlock;

internal record LlmTextBlock(string Text) : LlmContentBlock;

internal record LlmToolUseBlock(string Id, string Name, JsonNode? Input) : LlmContentBlock;

internal record LlmToolResultBlock(string ToolUseId, string Content) : LlmContentBlock;

/// <summary>A base64 inline document, such as a PDF.</summary>
internal record LlmDocumentBlock(string MediaType, string Base64Data) : LlmContentBlock;

/// <summary>
/// Anthropic only. Echoed back within the turn - with thinking on, a replayed assistant turn without
/// it is rejected. Never persisted.
/// </summary>
internal record LlmThinkingBlock(string Thinking, string? Signature) : LlmContentBlock;

/// <summary>Safety-redacted reasoning. Opaque, but still has to be echoed back.</summary>
internal record LlmRedactedThinkingBlock(string Data) : LlmContentBlock;

/// <summary>
/// Raw JSON of a block only its provider reads (Anthropic web search calls and results). Echoed
/// back verbatim within the turn - a later thinking block's signature covers it. Never persisted.
/// </summary>
internal record LlmProviderBlock(string Json) : LlmContentBlock;
