using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to toggle a single feature for the current tenant (used by tenant owner).
/// </summary>
public class UpdateTenantFeatureCommand : IAppRequest
{
    /// <summary>
    /// The feature to toggle.
    /// </summary>
    public TenantFeature Feature { get; set; }

    /// <summary>
    /// Whether to enable or disable the feature.
    /// </summary>
    public bool IsEnabled { get; set; }
}
