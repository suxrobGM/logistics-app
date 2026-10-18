namespace Logistics.Infrastructure.AI.Llm.Contracts;

internal record LlmResponse
{
    public required LlmMessage AssistantMessage { get; init; }
    public string? TextContent { get; init; }
    public required LlmStopReason StopReason { get; init; }
    public required List<LlmToolUseBlock> ToolCalls { get; init; }
    public required LlmTokenUsage Usage { get; init; }
}
