using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Driver license class. US uses CDL classes A/B/C; EU uses categories C/CE/D/DE/C1/C1E.
/// </summary>
public enum LicenseClass
{
    [Description("US CDL Class A")]
    UsClassA,

    [Description("US CDL Class B")]
    UsClassB,

    [Description("US CDL Class C")]
    UsClassC,

    [Description("EU Category C")]
    EuC,

    [Description("EU Category C+E")]
    EuCE,

    [Description("EU Category D")]
    EuD,

    [Description("EU Category D+E")]
    EuDE,

    [Description("EU Category C1")]
    EuC1,

    [Description("EU Category C1+E")]
    EuC1E
}
