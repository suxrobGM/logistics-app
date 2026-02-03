using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

/// <summary>
/// Command to update the default feature configuration for new tenants (used by admin).
/// </summary>
public class UpdateDefaultFeaturesCommand : IAppRequest
{
    /// <summary>
    /// The list of default feature updates.
    /// </summary>
    public List<DefaultFeatureUpdate> Features { get; set; } = [];
}

/// <summary>
/// Represents a default feature configuration update.
/// </summary>
public record DefaultFeatureUpdate(
    TenantFeature Feature,
    bool IsEnabledByDefault);
