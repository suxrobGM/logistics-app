using Logistics.Infrastructure.AI.Llm.Contracts;

namespace Logistics.Infrastructure.AI.Llm;

/// <summary>
/// Provider-agnostic interface for LLM API calls.
/// Implementations adapt specific SDKs (Anthropic, OpenAI-compatible, etc.) to a common contract.
/// </summary>
internal interface ILlmProvider
{
    Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct);
}
