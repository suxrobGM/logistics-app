using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Features;

namespace Logistics.Infrastructure.Persistence.Services.Feature;

internal class FeatureService(IMasterUnitOfWork masterUow) : IFeatureService
{
    private static readonly TenantFeature[] AllFeatures = Enum.GetValues<TenantFeature>();

    // Scope-lifetime memo: without it every [RequiresFeature] check costs 3-5 master-DB round trips.
    // Holds entities, not snapshots, so a writer's tracked edits stay visible through the cache.
    private readonly Dictionary<Guid, List<TenantFeatureConfig>> _tenantConfigCache = [];
    private readonly Dictionary<Guid, TenantAccess> _tenantAccessCache = [];

    public async Task<bool> IsFeatureEnabledAsync(Guid tenantId, TenantFeature feature)
    {
        var context = await GetContextAsync(tenantId);
        return context.IsEnabled(feature);
    }

    public async Task<Result> CheckFeatureAsync(Guid tenantId, TenantFeature feature)
    {
        var context = await GetContextAsync(tenantId);
        if (context.IsEnabled(feature))
        {
            return Result.Ok();
        }

        var featureName = feature.GetDescription();
        return context.IsAdminLocked(feature)
            ? Result.Fail(
                $"The '{featureName}' feature has been disabled by your platform administrator.",
                ErrorCodes.FeatureDisabledByAdmin)
            : Result.Fail(
                $"The '{featureName}' feature is not included in your current subscription plan. Please upgrade to access this feature.",
                ErrorCodes.FeatureNotInPlan);
    }

    public async Task<IReadOnlyList<TenantFeature>> GetEnabledFeaturesAsync(Guid tenantId)
    {
        var context = await GetContextAsync(tenantId);
        return [.. AllFeatures.Where(context.IsEnabled)];
    }

    public async Task ApplyPresetFeaturesAsync(Guid tenantId, IReadOnlyCollection<TenantPreset> presets)
    {
        var configs = await GetTenantConfigsAsync(tenantId);
        var configMap = configs.ToDictionary(c => c.Feature);
        var presetFeatures = TenantPresetCatalog.Resolve(presets);
        var now = DateTime.UtcNow;

        foreach (var feature in AllFeatures)
        {
            var isEnabled = presetFeatures.Contains(feature);

            if (!configMap.TryGetValue(feature, out var config))
            {
                config = new TenantFeatureConfig
                {
                    TenantId = tenantId,
                    Feature = feature,
                    IsEnabled = isEnabled,
                    UpdatedAt = now
                };
                await masterUow.Repository<TenantFeatureConfig>().AddAsync(config);
                // The cached list is the one reads use, so staged rows count before the caller saves.
                configs.Add(config);
            }
            else if (!config.IsAdminLocked && config.IsEnabled != isEnabled)
            {
                config.IsEnabled = isEnabled;
                config.UpdatedAt = now;
            }
        }
    }

    public async Task<IReadOnlyList<FeatureStatusDto>> GetAllFeatureStatusAsync(Guid tenantId)
    {
        var context = await GetContextAsync(tenantId);

        return
        [
            .. AllFeatures.Select(f => new FeatureStatusDto(
                f,
                f.GetDescription(),
                context.IsEnabled(f),
                context.IsAdminLocked(f),
                context.IsInPlan(f)))
        ];
    }

    private async Task<FeatureContext> GetContextAsync(Guid tenantId)
    {
        var configs = await GetTenantConfigsAsync(tenantId);
        return new FeatureContext(configs.ToDictionary(c => c.Feature), await GetTenantAccessAsync(tenantId));
    }

    private async Task<TenantAccess> GetTenantAccessAsync(Guid tenantId)
    {
        if (_tenantAccessCache.TryGetValue(tenantId, out var cached))
        {
            return cached;
        }

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId);
        var planFeatures = tenant is { IsSubscriptionRequired: true, Subscription: not null }
            ? await GetPlanFeaturesAsync(tenant.Subscription.PlanId)
            : null;
        var access = new TenantAccess(planFeatures, TenantPresetCatalog.Resolve(tenant?.Presets ?? []));

        _tenantAccessCache[tenantId] = access;
        return access;
    }

    private async Task<HashSet<TenantFeature>> GetPlanFeaturesAsync(Guid planId)
    {
        var planFeatures = await masterUow.Repository<PlanFeature>()
            .GetListAsync(pf => pf.PlanId == planId);
        return [.. planFeatures.Select(pf => pf.Feature)];
    }

    private async Task<List<TenantFeatureConfig>> GetTenantConfigsAsync(Guid tenantId)
    {
        if (_tenantConfigCache.TryGetValue(tenantId, out var cached))
        {
            return cached;
        }

        var configs = await masterUow.Repository<TenantFeatureConfig>()
            .GetListAsync(c => c.TenantId == tenantId);

        _tenantConfigCache[tenantId] = configs;
        return configs;
    }

    /// <summary>
    ///     Plan and preset inputs for one tenant. <see cref="PlanFeatures" /> is null when no plan gates
    ///     apply (no subscription required, or none yet). <see cref="PresetFeatures" /> answers for a
    ///     feature with no config row - one added to the enum after the tenant's presets were applied.
    /// </summary>
    private sealed record TenantAccess(HashSet<TenantFeature>? PlanFeatures, IReadOnlySet<TenantFeature> PresetFeatures);

    /// <summary>
    ///     One tenant's full resolution state; owns the precedence chain:
    ///     admin lock → plan gate → tenant config → tenant presets.
    /// </summary>
    private sealed record FeatureContext(
        Dictionary<TenantFeature, TenantFeatureConfig> ConfigMap,
        TenantAccess Access)
    {
        public bool IsEnabled(TenantFeature feature)
        {
            if (ConfigMap.TryGetValue(feature, out var config) && config.IsAdminLocked)
            {
                return config.IsEnabled;
            }

            if (!IsInPlan(feature))
            {
                return false;
            }

            if (config is not null)
            {
                return config.IsEnabled;
            }

            return Access.PresetFeatures.Contains(feature);
        }

        public bool IsAdminLocked(TenantFeature feature) =>
            ConfigMap.GetValueOrDefault(feature)?.IsAdminLocked ?? false;

        public bool IsInPlan(TenantFeature feature) =>
            Access.PlanFeatures is null || Access.PlanFeatures.Contains(feature);
    }
}
