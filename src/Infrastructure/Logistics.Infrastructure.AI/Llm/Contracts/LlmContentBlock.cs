using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Llm.Contracts;

/// <summary>
/// Base type for the polymorphic content blocks that make up an <see cref="LlmMessage"/>.
/// Providers map these to their SDK-native content representations.
/// </summary>
internal abstract record LlmContentBlock;

internal record LlmTextBlock(string Text) : LlmContentBlock;

internal record LlmToolUseBlock(string Id, string Name, JsonNode? Input) : LlmContentBlock;

internal record LlmToolResultBlock(string ToolUseId, string Content) : LlmContentBlock;

/// <summary>An inline document such as a PDF (base64-encoded). Mapped to provider-native document content.</summary>
internal record LlmDocumentBlock(string MediaType, string Base64Data) : LlmContentBlock;

/// <summary>
/// An Anthropic thinking block, kept so the tool loop can echo it back - with thinking on, an
/// assistant turn replayed without it is rejected. Only in-turn replay is required.
/// </summary>
internal record LlmThinkingBlock(string Thinking, string? Signature) : LlmContentBlock;
