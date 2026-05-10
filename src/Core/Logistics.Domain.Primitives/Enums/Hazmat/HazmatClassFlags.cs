using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Flag-enum variant of <see cref="HazmatClass"/> used on <c>AdrEquipment.AllowedClasses</c>
/// so a truck can be certified for multiple classes simultaneously (e.g., 3 + 6 + 8).
/// </summary>
[Flags]
public enum HazmatClassFlags
{
    None = 0,

    [Description("Class 1 - Explosives")]
    Class1 = 1 << 0,

    [Description("Class 2 - Gases")]
    Class2 = 1 << 1,

    [Description("Class 3 - Flammable Liquids")]
    Class3 = 1 << 2,

    [Description("Class 4 - Flammable Solids")]
    Class4 = 1 << 3,

    [Description("Class 5 - Oxidizers")]
    Class5 = 1 << 4,

    [Description("Class 6 - Toxic & Infectious")]
    Class6 = 1 << 5,

    [Description("Class 7 - Radioactive")]
    Class7 = 1 << 6,

    [Description("Class 8 - Corrosives")]
    Class8 = 1 << 7,

    [Description("Class 9 - Miscellaneous")]
    Class9 = 1 << 8
}

public static class HazmatClassFlagsExtensions
{
    /// <summary>
    /// Returns the matching flag bit for a single <see cref="HazmatClass"/>.
    /// </summary>
    public static HazmatClassFlags ToFlag(this HazmatClass hazmatClass)
    {
        return hazmatClass switch
        {
            HazmatClass.Class1 => HazmatClassFlags.Class1,
            HazmatClass.Class2 => HazmatClassFlags.Class2,
            HazmatClass.Class3 => HazmatClassFlags.Class3,
            HazmatClass.Class4 => HazmatClassFlags.Class4,
            HazmatClass.Class5 => HazmatClassFlags.Class5,
            HazmatClass.Class6 => HazmatClassFlags.Class6,
            HazmatClass.Class7 => HazmatClassFlags.Class7,
            HazmatClass.Class8 => HazmatClassFlags.Class8,
            HazmatClass.Class9 => HazmatClassFlags.Class9,
            _ => HazmatClassFlags.None
        };
    }

    private static readonly (HazmatClassFlags Flag, HazmatClass Class)[] FlagToClassMap =
    [
        (HazmatClassFlags.Class1, HazmatClass.Class1),
        (HazmatClassFlags.Class2, HazmatClass.Class2),
        (HazmatClassFlags.Class3, HazmatClass.Class3),
        (HazmatClassFlags.Class4, HazmatClass.Class4),
        (HazmatClassFlags.Class5, HazmatClass.Class5),
        (HazmatClassFlags.Class6, HazmatClass.Class6),
        (HazmatClassFlags.Class7, HazmatClass.Class7),
        (HazmatClassFlags.Class8, HazmatClass.Class8),
        (HazmatClassFlags.Class9, HazmatClass.Class9)
    ];

    /// <summary>
    /// Expands a flags value into the set of individual <see cref="HazmatClass"/> values it contains.
    /// </summary>
    public static HazmatClass[] ToClasses(this HazmatClassFlags flags) =>
        FlagToClassMap.Where(pair => flags.HasFlag(pair.Flag)).Select(pair => pair.Class).ToArray();

    /// <summary>
    /// Combines a set of <see cref="HazmatClass"/> values into a single flags bitfield.
    /// </summary>
    public static HazmatClassFlags ToFlags(this IEnumerable<HazmatClass>? classes) =>
        classes?.Aggregate(HazmatClassFlags.None, (acc, c) => acc | c.ToFlag()) ?? HazmatClassFlags.None;
}
