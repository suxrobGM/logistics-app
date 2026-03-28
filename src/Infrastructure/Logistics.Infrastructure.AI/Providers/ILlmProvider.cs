namespace Logistics.Infrastructure.AI.Providers;

/// <summary>
/// Provider-agnostic interface for LLM API calls.
/// Implementations adapt specific SDKs (Anthropic, OpenAI-compatible, etc.) to a common contract.
/// </summary>
internal interface ILlmProvider
{
    /// <summary>
    /// Sends a request to the LLM and returns the response.
    /// The request includes the model, system prompt, conversation history, tools, and other parameters.
    /// </summary>
    /// <param name="request">The LLM request details, including model, prompts, messages, tools, and parameters.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The LLM response.</returns>
    Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct);
}
