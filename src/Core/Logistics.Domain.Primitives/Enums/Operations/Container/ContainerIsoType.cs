using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Common ISO 6346 container size and type codes.
/// </summary>
public enum ContainerIsoType
{
    [Description("20' GP")]
    Gp20,

    [Description("40' GP")]
    Gp40,

    [Description("40' High Cube")]
    Hc40,

    [Description("20' Reefer")]
    Rf20,

    [Description("40' Reefer")]
    Rf40,

    [Description("20' Open Top")]
    Ot20,

    [Description("40' Open Top")]
    Ot40,

    [Description("20' Flat Rack")]
    Fr20,

    [Description("40' Flat Rack")]
    Fr40,

    [Description("45' High Cube")]
    Hc45,

    [Description("20' Tank")]
    Tk20
}
