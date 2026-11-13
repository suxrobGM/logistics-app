using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Single hazardous material class assigned to a load (UN/ADR classification 1–9).
/// </summary>
public enum HazmatClass
{
    [Description("Class 1 - Explosives")]
    Class1 = 1,

    [Description("Class 2 - Gases")]
    Class2 = 2,

    [Description("Class 3 - Flammable Liquids")]
    Class3 = 3,

    [Description("Class 4 - Flammable Solids")]
    Class4 = 4,

    [Description("Class 5 - Oxidizers")]
    Class5 = 5,

    [Description("Class 6 - Toxic & Infectious")]
    Class6 = 6,

    [Description("Class 7 - Radioactive")]
    Class7 = 7,

    [Description("Class 8 - Corrosives")]
    Class8 = 8,

    [Description("Class 9 - Miscellaneous")]
    Class9 = 9
}
