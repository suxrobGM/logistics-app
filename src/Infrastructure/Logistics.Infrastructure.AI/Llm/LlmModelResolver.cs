using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Llm;

/// <summary>
/// Resolves the global, admin-managed LLM model: system setting (<c>AI.Model</c>) → appsettings default.
/// The provider is derived from the model via <see cref="LlmModelCatalog"/> so it cannot drift.
/// Shared by the dispatch conversation builder and the one-shot <c>ILlmClient</c>.
/// </summary>
internal sealed class LlmModelResolver(
    ISystemSettingsService systemSettings,
    ILogger<LlmModelResolver> logger)
{
    /// <param name="preferredModelId">
    /// Optional per-call override (see <c>LlmCompletionRequest.ModelId</c>). Honoured only when the id
    /// is in the catalog AND that provider has an API key; otherwise the global model wins.
    /// </param>
    public async Task<LlmModelSelection> ResolveAsync(
        LlmOptions config,
        string? preferredModelId = null,
        CancellationToken ct = default)
    {
        if (ResolvePreferred(config, preferredModelId) is { } preferred)
        {
            return preferred;
        }

        var modelSetting = await systemSettings.GetAsync(AISettingsKeys.Model, ct);
        var modelInfo = LlmModelCatalog.Find(modelSetting);
        var provider = modelInfo?.Provider ?? config.DefaultProvider;
        var providerConfig = config.GetProviderConfig(provider);
        var model = modelInfo?.Id ?? providerConfig.Model;

        return new LlmModelSelection(model, provider, providerConfig);
    }

    private LlmModelSelection? ResolvePreferred(LlmOptions config, string? preferredModelId)
    {
        if (string.IsNullOrWhiteSpace(preferredModelId))
        {
            return null;
        }

        var info = LlmModelCatalog.Find(preferredModelId);
        if (info is null)
        {
            logger.LogWarning(
                "Preferred model '{PreferredModel}' is not in the catalog; falling back to the global model",
                preferredModelId);
            return null;
        }

        // A deployment may configure only one provider's key; without this the override would
        // hard-fail every run on such an install.
        var providerConfig = config.GetProviderConfig(info.Provider);
        if (string.IsNullOrWhiteSpace(providerConfig.ApiKey))
        {
            logger.LogWarning(
                "Preferred model '{PreferredModel}' needs provider {Provider}, which has no API key configured; falling back to the global model",
                info.Id, info.Provider);
            return null;
        }

        return new LlmModelSelection(info.Id, info.Provider, providerConfig);
    }
}
