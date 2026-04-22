using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Options;

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

    public int MaxTokens { get; set; } = 8192;
    public bool EnableExtendedThinking { get; set; }
    public int ThinkingBudgetTokens { get; set; } = 10000;

    /// <summary>
    /// When true, skips the per-tenant LlmEnabled check. Set to true in development environments.
    /// </summary>
    public bool BypassLlmGate { get; set; }

    /// <summary>
    /// Gets the provider config for the specified type, or throws if not configured.
    /// </summary>
    public LlmProviderOptions GetProviderConfig(LlmProvider type) =>
        Providers.GetValueOrDefault(type)
        ?? throw new InvalidOperationException($"LLM provider '{type}' is not configured. Add it to Llm:Providers:{type} in appsettings.");
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
