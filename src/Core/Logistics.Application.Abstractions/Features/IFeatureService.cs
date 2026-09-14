using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Features;

namespace Logistics.Application.Abstractions.Features;

/// <summary>
/// Service for managing feature toggles across tenants.
/// </summary>
public interface IFeatureService
{
    /// <summary>
    /// Checks if a specific feature is enabled for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID to check.</param>
    /// <param name="feature">The feature to check.</param>
    /// <returns>True if the feature is enabled, false otherwise.</returns>
    Task<bool> IsFeatureEnabledAsync(Guid tenantId, TenantFeature feature);

    /// <summary>
    /// Fails with <see cref="ErrorCodes.FeatureDisabledByAdmin"/> or <see cref="ErrorCodes.FeatureNotInPlan"/> when the
    /// feature is off. The client shows an upgrade prompt only for the second, so every feature gate should use this.
    /// </summary>
    Task<Result> CheckFeatureAsync(Guid tenantId, TenantFeature feature);

    /// <summary>
    /// Gets all enabled features for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>A list of enabled features.</returns>
    Task<IReadOnlyList<TenantFeature>> GetEnabledFeaturesAsync(Guid tenantId);

    /// <summary>
    /// Gets all feature configurations for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>A list of feature status objects with enable/lock state.</returns>
    Task<IReadOnlyList<FeatureStatusDto>> GetAllFeatureStatusAsync(Guid tenantId);

    /// <summary>
    /// Gets the default feature configurations applied to new tenants.
    /// </summary>
    Task<IReadOnlyList<DefaultFeatureStatusDto>> GetDefaultFeaturesAsync();

    /// <summary>
    /// Stages a tenant's feature configurations from its presets on the master unit of work; the caller saves.
    /// Admin-locked configurations keep their value.
    /// </summary>
    Task ApplyPresetFeaturesAsync(Guid tenantId, IReadOnlyCollection<TenantPreset> presets);
}
