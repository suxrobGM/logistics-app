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
    public LlmProvider DefaultProvider { get; set; } = LlmProvider.DeepSeek;

    /// <summary>
    /// Provider-specific configurations keyed by provider type.
    /// </summary>
    public Dictionary<LlmProvider, LlmProviderOptions> Providers { get; set; } = [];

    public int MaxTokens { get; set; } = 16384;
    public bool EnableExtendedThinking { get; set; }
    public int ThinkingBudgetTokens { get; set; } = 16384;

    /// <summary>
    /// When true, skips the per-tenant LlmEnabled check. Set to true in development environments.
    /// </summary>
    public bool BypassLlmGate { get; set; }

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
