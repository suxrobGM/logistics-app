using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Attributes;

/// <summary>
/// Marks a command or query as requiring a tenant feature to be enabled.
/// Used by the FeatureCheckBehavior pipeline to enforce feature toggles.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class RequiresFeatureAttribute(params TenantFeature[] features) : Attribute
{
    /// <summary>
    /// The features that admit the request: **any** one enabled is enough. When none is, the first
    /// one's error is returned, so list the feature whose upgrade prompt fits best first.
    /// </summary>
    public TenantFeature[] Features { get; } = features;
}
