using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AI;

/// <summary>
/// The bound <c>Llm</c> section. In Abstractions, not Infrastructure.AI, because the Application
/// layer reads it too - without that it fell back to raw <c>IConfiguration["Llm:..."]</c> keys.
/// </summary>
public class LlmOptions
{
    public const string SectionName = "Llm";

    /// <summary>
    /// The default provider to use when no tenant override is set.
    /// </summary>
    public LlmProvider DefaultProvider { get; set; } = LlmProvider.OpenAI;

    /// <summary>
    /// Provider-specific configurations keyed by provider type.
    /// </summary>
    public Dictionary<LlmProvider, LlmProviderOptions> Providers { get; set; } = [];

    public int MaxTokens { get; set; } = 16384;

    /// <summary>
    /// Reasoning depth used when the admin has not set <c>AI.ReasoningEffort</c> yet.
    /// </summary>
    public ReasoningEffort DefaultReasoningEffort { get; set; } = ReasoningEffort.None;

    /// <summary>
    /// Per-call ceiling on an LLM HTTP request. Generous because a high reasoning effort on a large
    /// prompt legitimately takes minutes - this is a backstop against a wedged connection, not a
    /// latency target. The agent loop runs up to 25 iterations, so without it a single stuck
    /// session can hang for 25x the SDK default.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Ceiling on one whole agent session, across all its iterations. Stops a session that keeps
    /// making individually-fast calls forever.
    /// </summary>
    public int SessionTimeoutMinutes { get; set; } = 30;

    /// <summary>
    /// When true, skips the per-tenant AIEnabled check. Set to true in development environments.
    /// </summary>
    public bool BypassAIGate { get; set; }

    /// <summary>
    /// Cheap-tier model for background learning passes. Falls back to the global model when unset,
    /// unknown, or its provider has no API key.
    /// </summary>
    public string? PolicyLearningModel { get; set; }

    /// <summary>
    /// Gets the provider config for the specified type, or throws if not configured.
    /// </summary>
    public LlmProviderOptions GetProviderConfig(LlmProvider type) =>
        Providers.GetValueOrDefault(type)
        ?? throw new InvalidOperationException($"LLM provider '{type}' is not configured. Add it to Llm:Providers:{type} in appsettings.");

    /// <summary>
    /// A provider's configured model, or null if it is not configured. Unlike
    /// <see cref="GetProviderConfig"/> this does not throw.
    /// </summary>
    public string? FindProviderModel(LlmProvider type) =>
        Providers.GetValueOrDefault(type)?.Model;
}

public class LlmProviderOptions
{
    public required string ApiKey { get; set; }
    public string Model { get; set; } = "";

    /// <summary>
    /// Base URL override for OpenAI-compatible providers (DeepSeek, GLM, Groq, etc.).
    /// Not used by the Anthropic provider.
    /// </summary>
    public string? BaseUrl { get; set; }
}
