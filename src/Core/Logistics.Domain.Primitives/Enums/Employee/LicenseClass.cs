using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Driver license class. US uses CDL classes A/B/C; EU uses categories C/CE/D/DE/C1/C1E.
/// </summary>
public enum LicenseClass
{
    [Description("US CDL Class A")]
    USClassA,

    [Description("US CDL Class B")]
    USClassB,

    [Description("US CDL Class C")]
    USClassC,

    [Description("EU Category C")]
    EUC,

    [Description("EU Category C+E")]
    EUCE,

    [Description("EU Category D")]
    EUD,

    [Description("EU Category D+E")]
    EUDE,

    [Description("EU Category C1")]
    EUC1,

    [Description("EU Category C1+E")]
    EUC1E
}
