using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Company type an admin assigns to a tenant. Presets combine; <c>TenantPresetCatalog</c> maps them
/// to features.
/// </summary>
public enum TenantPreset
{
    GeneralFreight,
    CarHauler,

    [Description("Intermodal / Drayage")]
    Intermodal,

    /// <summary>A one-person carrier. Modifies the cargo presets, so it never stands alone.</summary>
    [Description("Solo Owner-Operator")]
    SoloOperator
}
