using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Attributes;

/// <summary>
/// Marks a command or query as requiring a specific tenant feature to be enabled.
/// Used by the FeatureCheckBehavior pipeline to enforce feature toggles.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class RequiresFeatureAttribute(TenantFeature feature) : Attribute
{
    /// <summary>
    /// The feature that must be enabled for the tenant to execute this request.
    /// </summary>
    public TenantFeature Feature { get; } = feature;
}
