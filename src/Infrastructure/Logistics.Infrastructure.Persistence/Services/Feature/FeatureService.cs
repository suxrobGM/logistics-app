using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Services;

internal class FeatureService(IMasterUnitOfWork masterUow) : IFeatureService
{
    private static readonly TenantFeature[] allFeatures = Enum.GetValues<TenantFeature>();

    public async Task<bool> IsFeatureEnabledAsync(Guid tenantId, TenantFeature feature)
    {
        var config = await masterUow.Repository<TenantFeatureConfig>()
            .GetAsync(c => c.TenantId == tenantId && c.Feature == feature);

        if (config is not null)
        {
            return config.IsEnabled;
        }

        var defaultConfig = await masterUow.Repository<DefaultFeatureConfig>()
            .GetAsync(c => c.Feature == feature);

        return defaultConfig?.IsEnabledByDefault ?? true;
    }

    public async Task<IReadOnlyList<TenantFeature>> GetEnabledFeaturesAsync(Guid tenantId)
    {
        var (configMap, defaultMap) = await LoadFeatureMapsAsync(tenantId);

        return [.. allFeatures.Where(f => IsFeatureEnabled(f, configMap, defaultMap))];
    }

    public async Task InitializeFeaturesForTenantAsync(Guid tenantId)
    {
        var existingConfigs = await masterUow.Repository<TenantFeatureConfig>()
            .GetListAsync(c => c.TenantId == tenantId);
        var existingFeatures = existingConfigs.Select(c => c.Feature).ToHashSet();

        var defaults = await masterUow.Repository<DefaultFeatureConfig>().GetListAsync();
        var defaultMap = defaults.ToDictionary(d => d.Feature, d => d.IsEnabledByDefault);

        var newConfigs = allFeatures
            .Where(f => !existingFeatures.Contains(f))
            .Select(f => new TenantFeatureConfig
            {
                TenantId = tenantId,
                Feature = f,
                IsEnabled = defaultMap.GetValueOrDefault(f, true),
                IsAdminLocked = false,
                UpdatedAt = DateTime.UtcNow
            });

        foreach (var config in newConfigs)
        {
            await masterUow.Repository<TenantFeatureConfig>().AddAsync(config);
        }

        await masterUow.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<FeatureStatusDto>> GetAllFeatureStatusAsync(Guid tenantId)
    {
        var (configMap, defaultMap) = await LoadFeatureMapsAsync(tenantId);

        return
        [
            .. allFeatures.Select(f => new FeatureStatusDto(
                f,
                f.GetDescription(),
                IsFeatureEnabled(f, configMap, defaultMap),
                configMap.GetValueOrDefault(f)?.IsAdminLocked ?? false))
        ];
    }

    public async Task<IReadOnlyList<DefaultFeatureStatusDto>> GetDefaultFeaturesAsync()
    {
        var defaults = await masterUow.Repository<DefaultFeatureConfig>().GetListAsync();
        var defaultMap = defaults.ToDictionary(d => d.Feature, d => d.IsEnabledByDefault);

        return
        [
            .. allFeatures.Select(f => new DefaultFeatureStatusDto(
                f,
                f.GetDescription(),
                defaultMap.GetValueOrDefault(f, true)))
        ];
    }

    private async
        Task<(Dictionary<TenantFeature, TenantFeatureConfig> configs, Dictionary<TenantFeature, bool> defaults)>
        LoadFeatureMapsAsync(Guid tenantId)
    {
        var configs = await masterUow.Repository<TenantFeatureConfig>()
            .GetListAsync(c => c.TenantId == tenantId);
        var defaults = await masterUow.Repository<DefaultFeatureConfig>()
            .GetListAsync();

        return (
            configs.ToDictionary(c => c.Feature),
            defaults.ToDictionary(d => d.Feature, d => d.IsEnabledByDefault)
        );
    }

    private static bool IsFeatureEnabled(
        TenantFeature feature,
        Dictionary<TenantFeature, TenantFeatureConfig> configMap,
        Dictionary<TenantFeature, bool> defaultMap)
    {
        if (configMap.TryGetValue(feature, out var config))
        {
            return config.IsEnabled;
        }

        return defaultMap.GetValueOrDefault(feature, true);
    }
}
