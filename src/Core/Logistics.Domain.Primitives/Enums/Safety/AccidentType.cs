using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum AccidentType
{
    Collision,
    Rollover,
    Jackknife,
    RunOffRoad,
    RearEnd,
    Sideswipe,

    [Description("Head-On")]
    HeadOn,

    HitAndRun,
    PedestrianInvolved,
    PropertyDamageOnly,
    CargoSpill,
    Other
}
