namespace Logistics.Infrastructure.AI.Models;

/// <summary>
/// The provider-agnostic response returned from an LLM call.
/// </summary>
internal record LlmResponse
{
    public required LlmMessage AssistantMessage { get; init; }
    public string? TextContent { get; init; }
    public required string StopReason { get; init; }
    public required List<LlmToolUseBlock> ToolCalls { get; init; }
    public required LlmTokenUsage Usage { get; init; }
}
