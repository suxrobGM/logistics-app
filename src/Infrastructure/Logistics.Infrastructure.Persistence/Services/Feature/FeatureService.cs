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
    private readonly Dictionary<Guid, List<TenantFeatureConfig>> tenantConfigCache = [];
    private readonly Dictionary<Guid, HashSet<TenantFeature>?> planFeatureCache = [];
    private List<DefaultFeatureConfig>? defaultConfigCache;

    public async Task<bool> IsFeatureEnabledAsync(Guid tenantId, TenantFeature feature)
    {
        var (configMap, defaultMap) = await LoadFeatureMapsAsync(tenantId);
        var planFeatures = await GetPlanFeaturesForTenantAsync(tenantId);

        return IsFeatureEnabled(feature, configMap, defaultMap, planFeatures);
    }

    public async Task<IReadOnlyList<TenantFeature>> GetEnabledFeaturesAsync(Guid tenantId)
    {
        var (configMap, defaultMap) = await LoadFeatureMapsAsync(tenantId);
        var planFeatures = await GetPlanFeaturesForTenantAsync(tenantId);

        return [.. AllFeatures.Where(f => IsFeatureEnabled(f, configMap, defaultMap, planFeatures))];
    }

    public async Task InitializeFeaturesForTenantAsync(Guid tenantId)
    {
        var existingConfigs = await GetTenantConfigsAsync(tenantId);
        var existingFeatures = existingConfigs.Select(c => c.Feature).ToHashSet();

        var defaults = await GetDefaultConfigsAsync();
        var defaultMap = defaults.ToDictionary(d => d.Feature, d => d.IsEnabledByDefault);
        var planFeatures = await GetPlanFeaturesForTenantAsync(tenantId);

        var newConfigs = AllFeatures
            .Where(f => !existingFeatures.Contains(f))
            .Select(f => new TenantFeatureConfig
            {
                TenantId = tenantId,
                Feature = f,
                IsEnabled = (planFeatures is null || planFeatures.Contains(f))
                    && defaultMap.GetValueOrDefault(f, true),
                IsAdminLocked = false,
                UpdatedAt = DateTime.UtcNow
            });

        foreach (var config in newConfigs)
        {
            await masterUow.Repository<TenantFeatureConfig>().AddAsync(config);
        }

        await masterUow.SaveChangesAsync();

        // Cached before these rows existed.
        tenantConfigCache.Remove(tenantId);
    }

    public async Task<IReadOnlyList<FeatureStatusDto>> GetAllFeatureStatusAsync(Guid tenantId)
    {
        var (configMap, defaultMap) = await LoadFeatureMapsAsync(tenantId);
        var planFeatures = await GetPlanFeaturesForTenantAsync(tenantId);

        return
        [
            .. AllFeatures.Select(f => new FeatureStatusDto(
                f,
                f.GetDescription(),
                IsFeatureEnabled(f, configMap, defaultMap, planFeatures),
                configMap.GetValueOrDefault(f)?.IsAdminLocked ?? false,
                planFeatures is null || planFeatures.Contains(f)))
        ];
    }

    public async Task<IReadOnlyList<DefaultFeatureStatusDto>> GetDefaultFeaturesAsync()
    {
        var defaults = await GetDefaultConfigsAsync();
        var defaultMap = defaults.ToDictionary(d => d.Feature, d => d.IsEnabledByDefault);

        return
        [
            .. AllFeatures.Select(f => new DefaultFeatureStatusDto(
                f,
                f.GetDescription(),
                defaultMap.GetValueOrDefault(f, true)))
        ];
    }

    private async Task<HashSet<TenantFeature>?> GetPlanFeaturesForTenantAsync(Guid tenantId)
    {
        // null is a real answer ("all features allowed"), so probe on key presence.
        if (planFeatureCache.TryGetValue(tenantId, out var cached))
        {
            return cached;
        }

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId);
        HashSet<TenantFeature>? features = null;

        if (tenant is { IsSubscriptionRequired: true, Subscription: not null })
        {
            features = await GetPlanFeaturesAsync(tenant.Subscription.PlanId);
        }

        planFeatureCache[tenantId] = features;
        return features;
    }

    private async Task<HashSet<TenantFeature>> GetPlanFeaturesAsync(Guid planId)
    {
        var planFeatures = await masterUow.Repository<PlanFeature>()
            .GetListAsync(pf => pf.PlanId == planId);
        return [.. planFeatures.Select(pf => pf.Feature)];
    }

    private async Task<List<TenantFeatureConfig>> GetTenantConfigsAsync(Guid tenantId)
    {
        if (tenantConfigCache.TryGetValue(tenantId, out var cached))
        {
            return cached;
        }

        var configs = await masterUow.Repository<TenantFeatureConfig>()
            .GetListAsync(c => c.TenantId == tenantId);

        tenantConfigCache[tenantId] = configs;
        return configs;
    }

    private async Task<List<DefaultFeatureConfig>> GetDefaultConfigsAsync() =>
        defaultConfigCache ??= await masterUow.Repository<DefaultFeatureConfig>().GetListAsync();

    private async
        Task<(Dictionary<TenantFeature, TenantFeatureConfig> configs, Dictionary<TenantFeature, bool> defaults)>
        LoadFeatureMapsAsync(Guid tenantId)
    {
        var configs = await GetTenantConfigsAsync(tenantId);
        var defaults = await GetDefaultConfigsAsync();

        return (
            configs.ToDictionary(c => c.Feature),
            defaults.ToDictionary(d => d.Feature, d => d.IsEnabledByDefault)
        );
    }

    private static bool IsFeatureEnabled(
        TenantFeature feature,
        Dictionary<TenantFeature, TenantFeatureConfig> configMap,
        Dictionary<TenantFeature, bool> defaultMap,
        HashSet<TenantFeature>? planFeatures)
    {
        // Admin-locked overrides take highest priority
        if (configMap.TryGetValue(feature, out var config) && config.IsAdminLocked)
        {
            return config.IsEnabled;
        }

        // Plan-based gating (null planFeatures means all features allowed)
        if (planFeatures is not null && !planFeatures.Contains(feature))
        {
            return false;
        }

        // Tenant-specific non-locked override
        if (config is not null)
        {
            return config.IsEnabled;
        }

        return defaultMap.GetValueOrDefault(feature, true);
    }
}
