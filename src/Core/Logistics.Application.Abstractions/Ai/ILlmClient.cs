using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Ai;

/// <summary>
/// Application-facing entry point for one-shot LLM calls (no agent loop, no tools).
/// Resolves the globally configured model, sends a single prompt — optionally with inline
/// documents/images — and reports token usage and estimated cost.
/// Pairs with the internal per-SDK <c>ILlmProvider</c> adapters (provider = adapter, client = entry point).
/// </summary>
public interface ILlmClient
{
    /// <summary>
    /// Sends a single completion request and returns the model's text response.
    /// </summary>
    Task<Result<LlmCompletionResult>> CompleteAsync(LlmCompletionRequest request, CancellationToken ct = default);
}

/// <summary>
/// A single LLM completion request.
/// </summary>
public sealed record LlmCompletionRequest
{
    /// <summary>The system prompt that defines the task and output contract.</summary>
    public required string SystemPrompt { get; init; }

    /// <summary>The user content (e.g. the text extracted from a document, plus instructions).</summary>
    public required string UserText { get; init; }

    /// <summary>Inline documents (e.g. a PDF) sent alongside the text. Empty for text-only calls.</summary>
    public IReadOnlyList<LlmInlineDocument> Documents { get; init; } = [];

    /// <summary>Maximum output tokens for the response.</summary>
    public int MaxTokens { get; init; } = 4096;
}

/// <summary>
/// An inline document attached to a completion request.
/// </summary>
/// <param name="MediaType">The IANA media type, e.g. "application/pdf".</param>
/// <param name="Data">The raw document bytes.</param>
public sealed record LlmInlineDocument(string MediaType, byte[] Data);

/// <summary>
/// The result of a successful LLM completion.
/// </summary>
/// <param name="Text">The model's text response.</param>
/// <param name="ModelUsed">The model id that served the request.</param>
/// <param name="InputTokens">Input tokens consumed.</param>
/// <param name="OutputTokens">Output tokens produced.</param>
/// <param name="EstimatedCostUsd">Estimated cost in USD for this call.</param>
public sealed record LlmCompletionResult(
    string Text,
    string ModelUsed,
    int InputTokens,
    int OutputTokens,
    decimal EstimatedCostUsd);
