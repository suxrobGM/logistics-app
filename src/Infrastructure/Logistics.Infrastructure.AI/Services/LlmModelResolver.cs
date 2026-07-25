using Logistics.Application.Abstractions.AiDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Resolves the global, admin-managed LLM model: system setting (<c>Ai.Model</c>) → appsettings default.
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
        if (TryResolvePreferred(config, preferredModelId, out var preferred))
        {
            return preferred;
        }

        var modelSetting = await systemSettings.GetAsync(AiSettingsKeys.Model, ct);
        var modelInfo = LlmModelCatalog.Find(modelSetting);
        var provider = modelInfo?.Provider ?? config.DefaultProvider;
        var providerConfig = config.GetProviderConfig(provider);
        var model = modelInfo?.Id ?? providerConfig.Model;

        return new LlmModelSelection(model, provider, providerConfig);
    }

    private bool TryResolvePreferred(
        LlmOptions config,
        string? preferredModelId,
        out LlmModelSelection selection)
    {
        selection = null!;

        if (string.IsNullOrWhiteSpace(preferredModelId))
        {
            return false;
        }

        var info = LlmModelCatalog.Find(preferredModelId);
        if (info is null)
        {
            logger.LogWarning(
                "Preferred model '{PreferredModel}' is not in the catalog; falling back to the global model",
                preferredModelId);
            return false;
        }

        // A deployment may configure only one provider's key; without this the override would
        // hard-fail every run on such an install.
        var providerConfig = config.GetProviderConfig(info.Provider);
        if (string.IsNullOrWhiteSpace(providerConfig.ApiKey))
        {
            logger.LogWarning(
                "Preferred model '{PreferredModel}' needs provider {Provider}, which has no API key configured; falling back to the global model",
                info.Id, info.Provider);
            return false;
        }

        selection = new LlmModelSelection(info.Id, info.Provider, providerConfig);
        return true;
    }
}

/// <summary>The resolved model, its provider, and that provider's configuration.</summary>
internal sealed record LlmModelSelection(string Model, LlmProvider Provider, LlmProviderOptions ProviderConfig);
