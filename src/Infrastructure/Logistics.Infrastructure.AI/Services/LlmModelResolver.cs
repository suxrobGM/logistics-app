using Logistics.Application.Abstractions.AiDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Resolves the global, admin-managed LLM model: system setting (<c>Ai.Model</c>) → appsettings default.
/// The provider is derived from the model via <see cref="LlmModelCatalog"/> so it cannot drift.
/// Shared by the dispatch conversation builder and the one-shot <c>ILlmClient</c>.
/// </summary>
internal sealed class LlmModelResolver(ISystemSettingsService systemSettings)
{
    public async Task<LlmModelSelection> ResolveAsync(LlmOptions config, CancellationToken ct = default)
    {
        var modelSetting = await systemSettings.GetAsync(AiSettingsKeys.Model, ct);
        var modelInfo = LlmModelCatalog.Find(modelSetting);
        var provider = modelInfo?.Provider ?? config.DefaultProvider;
        var providerConfig = config.GetProviderConfig(provider);
        var model = modelInfo?.Id ?? providerConfig.Model;

        return new LlmModelSelection(model, provider, providerConfig);
    }
}

/// <summary>The resolved model, its provider, and that provider's configuration.</summary>
internal sealed record LlmModelSelection(string Model, LlmProvider Provider, LlmProviderOptions ProviderConfig);
