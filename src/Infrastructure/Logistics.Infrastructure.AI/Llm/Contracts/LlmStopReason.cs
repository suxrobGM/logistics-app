namespace Logistics.Infrastructure.AI.Llm.Contracts;

/// <summary>Why the model stopped. Each provider maps its own vocabulary onto this.</summary>
internal enum LlmStopReason
{
    EndTurn,

    ToolUse,

    /// <summary>Hit the output cap: any tool call in the reply may be cut off and must not run.</summary>
    MaxTokens,

    /// <summary>Anthropic's server tool loop paused; resend the conversation as is to resume.</summary>
    PauseTurn,

    Refusal
}
