using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// What an accessorial rate is multiplied by on the invoice.
/// </summary>
public enum AccessorialUnit
{
    Flat,

    [Description("Per hour")]
    PerHour,

    /// <summary>Per mile or kilometre, following the tenant's distance unit.</summary>
    [Description("Per mile / km")]
    PerDistance,

    [Description("Per stop")]
    PerStop,

    [Description("Per day")]
    PerDay
}
