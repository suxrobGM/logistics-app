using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Services;

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
    /// Initializes feature configurations for a new tenant based on defaults.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    Task InitializeFeaturesForTenantAsync(Guid tenantId);
}
