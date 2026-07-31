namespace Logistics.Application.Abstractions.AI;

/// <summary>
/// <see cref="SystemSettings"/> keys for the platform-wide AI dispatch configuration.
/// </summary>
public static class AISettingsKeys
{
    /// <summary>
    /// The globally selected dispatch model id (e.g. "gpt-5.6-luna"). The provider is derived
    /// from the model via <c>LlmModelCatalog</c>, so it is not stored separately.
    /// </summary>
    public const string Model = "AI.Model";

    /// <summary>
    /// The global <c>ReasoningEffort</c> level for both agents, stored as the enum name
    /// (e.g. "None", "High"). Models without a reasoning control ignore it.
    /// </summary>
    public const string ReasoningEffort = "AI.ReasoningEffort";
}
