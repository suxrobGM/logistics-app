using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to update multiple feature settings for a tenant (used by admin).
/// </summary>
public class UpdateTenantFeaturesAdminCommand : IAppRequest
{
    /// <summary>
    /// The tenant ID to update features for.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// The list of feature updates.
    /// </summary>
    public List<FeatureUpdate> Features { get; set; } = [];
}

/// <summary>
/// Represents a feature update with enable and lock status.
/// </summary>
public record FeatureUpdate(
    TenantFeature Feature,
    bool IsEnabled,
    bool IsAdminLocked);
