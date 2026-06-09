namespace Logistics.Application.Abstractions.AiDispatch;

/// <summary>
/// <see cref="SystemSettings"/> keys for the platform-wide AI dispatch configuration.
/// </summary>
public static class AiSettingsKeys
{
    /// <summary>
    /// The globally selected dispatch model id (e.g. "deepseek-v4-flash"). The provider is derived
    /// from the model via <c>LlmModelCatalog</c>, so it is not stored separately.
    /// </summary>
    public const string Model = "Ai.Model";

    /// <summary>
    /// Whether extended thinking is enabled for the dispatch agent ("true"/"false").
    /// Only honored by providers/models that support it (e.g. Anthropic); ignored otherwise.
    /// </summary>
    public const string ExtendedThinking = "Ai.ExtendedThinking";
}
