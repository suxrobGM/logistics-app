using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.AI.Providers;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// The setup both agent surfaces share: resolve the global model, build its provider, assert the
/// API key exists, and read the tenant's feature set once.
/// </summary>
internal sealed class LlmSessionSetup(
    IFeatureService featureService,
    LlmProviderFactory providerFactory,
    LlmModelResolver modelResolver,
    ITenantUnitOfWork tenantUow)
{
    public async Task<LlmSessionContext> ResolveAsync(LlmOptions config, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        // The global model is admin-managed: system setting → appsettings default. The provider is
        // derived from the model via the catalog, so it can't drift. No per-request override on
        // purpose - ILlmClient.ModelId is for one-shot background calls.
        var selection = await modelResolver.ResolveAsync(config, ct: ct);
        var provider = providerFactory.Create(selection.Provider);

        // Worded to match LlmErrorSanitizer.ForSession, which classifies anything
        // mentioning "api key" as a configuration fault before it reaches the tenant.
        if (string.IsNullOrWhiteSpace(selection.ProviderConfig.ApiKey))
            throw new InvalidOperationException("LLM API key is not configured.");

        // One resolve for every gate - IsFeatureEnabledAsync costs master-DB round trips each time.
        var enabledFeatures = (await featureService.GetEnabledFeaturesAsync(tenant.Id)).ToHashSet();

        return new LlmSessionContext(tenant, provider, selection, enabledFeatures);
    }
}
