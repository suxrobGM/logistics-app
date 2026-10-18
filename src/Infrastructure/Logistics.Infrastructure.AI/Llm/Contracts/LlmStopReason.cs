namespace Logistics.Infrastructure.AI.Llm.Contracts;

/// <summary>Why the model stopped. Each provider maps its own vocabulary onto this.</summary>
internal enum LlmStopReason
{
    /// <summary>The model finished its reply.</summary>
    EndTurn,

    /// <summary>The model is waiting on the tool calls in its reply.</summary>
    ToolUse,

    /// <summary>
    /// The reply hit the output-token cap. Any tool call in it may be cut off mid-argument, so it
    /// must not run.
    /// </summary>
    MaxTokens,

    /// <summary>
    /// Anthropic only: a server-side tool loop (web search) paused. Resend the conversation as it
    /// stands and the server resumes.
    /// </summary>
    PauseTurn,

    /// <summary>The provider declined to answer.</summary>
    Refusal
}
