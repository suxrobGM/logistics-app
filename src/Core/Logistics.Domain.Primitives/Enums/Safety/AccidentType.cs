using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum AccidentType
{
    [Description("Collision")] [EnumMember(Value = "collision")]
    Collision,

    [Description("Rollover")] [EnumMember(Value = "rollover")]
    Rollover,

    [Description("Jackknife")] [EnumMember(Value = "jackknife")]
    Jackknife,

    [Description("Run Off Road")] [EnumMember(Value = "run_off_road")]
    RunOffRoad,

    [Description("Rear End")] [EnumMember(Value = "rear_end")]
    RearEnd,

    [Description("Sideswipe")] [EnumMember(Value = "sideswipe")]
    Sideswipe,

    [Description("Head-On")] [EnumMember(Value = "head_on")]
    HeadOn,

    [Description("Hit and Run")] [EnumMember(Value = "hit_and_run")]
    HitAndRun,

    [Description("Pedestrian Involved")] [EnumMember(Value = "pedestrian")]
    PedestrianInvolved,

    [Description("Property Damage Only")] [EnumMember(Value = "property_damage")]
    PropertyDamageOnly,

    [Description("Cargo Spill")] [EnumMember(Value = "cargo_spill")]
    CargoSpill,

    [Description("Other")] [EnumMember(Value = "other")]
    Other
}
