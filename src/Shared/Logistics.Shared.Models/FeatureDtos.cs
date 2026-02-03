using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Represents the status of a feature for a tenant.
/// </summary>
public record FeatureStatusDto(
    TenantFeature Feature,
    string Name,
    bool IsEnabled,
    bool IsAdminLocked);

/// <summary>
/// Represents the default configuration for a feature.
/// </summary>
public record DefaultFeatureStatusDto(
    TenantFeature Feature,
    string Name,
    bool IsEnabledByDefault);
