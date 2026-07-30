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
    private readonly Dictionary<Guid, PlanAccess> planAccessCache = [];
    private List<DefaultFeatureConfig>? defaultConfigCache;

    public async Task<bool> IsFeatureEnabledAsync(Guid tenantId, TenantFeature feature)
    {
        var context = await GetContextAsync(tenantId);
        return context.IsEnabled(feature);
    }

    public async Task<IReadOnlyList<TenantFeature>> GetEnabledFeaturesAsync(Guid tenantId)
    {
        var context = await GetContextAsync(tenantId);
        return [.. AllFeatures.Where(context.IsEnabled)];
    }

    public async Task InitializeFeaturesForTenantAsync(Guid tenantId)
    {
        var context = await GetContextAsync(tenantId);

        // A feature without a config row initializes to whatever resolution already says for it,
        // so the stored value and the computed one cannot disagree.
        var newConfigs = AllFeatures
            .Where(f => !context.ConfigMap.ContainsKey(f))
            .Select(f => new TenantFeatureConfig
            {
                TenantId = tenantId,
                Feature = f,
                IsEnabled = context.IsEnabled(f),
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

    public async Task<IReadOnlyList<DefaultFeatureStatusDto>> GetDefaultFeaturesAsync()
    {
        var defaultMap = await GetDefaultMapAsync();

        return
        [
            .. AllFeatures.Select(f => new DefaultFeatureStatusDto(
                f,
                f.GetDescription(),
                defaultMap.GetValueOrDefault(f, true)))
        ];
    }

    private async Task<FeatureContext> GetContextAsync(Guid tenantId)
    {
        var configs = await GetTenantConfigsAsync(tenantId);
        return new FeatureContext(
            configs.ToDictionary(c => c.Feature),
            await GetDefaultMapAsync(),
            await GetPlanAccessAsync(tenantId));
    }

    private async Task<PlanAccess> GetPlanAccessAsync(Guid tenantId)
    {
        if (planAccessCache.TryGetValue(tenantId, out var cached))
        {
            return cached;
        }

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId);
        var access = tenant switch
        {
            { IsSubscriptionRequired: false } => new PlanAccess(true, null),
            { Subscription: not null } => new PlanAccess(false, await GetPlanFeaturesAsync(tenant.Subscription.PlanId)),
            _ => new PlanAccess(false, null)
        };

        planAccessCache[tenantId] = access;
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

    private async Task<Dictionary<TenantFeature, bool>> GetDefaultMapAsync() =>
        (await GetDefaultConfigsAsync()).ToDictionary(d => d.Feature, d => d.IsEnabledByDefault);

    /// <summary>
    ///     Plan-derived access for one tenant. <see cref="IsUnrestricted" /> is set when the tenant
    ///     does not require a subscription - such tenants get every feature, like the top plan.
    ///     <see cref="PlanFeatures" /> is null when no plan gates apply.
    /// </summary>
    private sealed record PlanAccess(bool IsUnrestricted, HashSet<TenantFeature>? PlanFeatures);

    /// <summary>
    ///     One tenant's full resolution state; owns the precedence chain:
    ///     admin lock → plan gate → tenant override → platform default.
    /// </summary>
    private sealed record FeatureContext(
        Dictionary<TenantFeature, TenantFeatureConfig> ConfigMap,
        Dictionary<TenantFeature, bool> DefaultMap,
        PlanAccess PlanAccess)
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

            // Unrestricted tenants skip the platform defaults too: a default-off plan differentiator
            // (e.g. AICopilot) would otherwise stay locked with no plan to ever grant it.
            return PlanAccess.IsUnrestricted || DefaultMap.GetValueOrDefault(feature, true);
        }

        public bool IsAdminLocked(TenantFeature feature) =>
            ConfigMap.GetValueOrDefault(feature)?.IsAdminLocked ?? false;

        public bool IsInPlan(TenantFeature feature) =>
            PlanAccess.PlanFeatures is null || PlanAccess.PlanFeatures.Contains(feature);
    }
}
