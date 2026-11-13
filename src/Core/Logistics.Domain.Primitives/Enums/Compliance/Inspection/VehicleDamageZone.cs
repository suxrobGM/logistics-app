using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// A zone on the top-view vehicle diagram that car hauler drivers tap to mark damage. Left and
/// right are the driver's side and the passenger's side of a left-hand-drive vehicle.
/// Each zone rolls up to a <see cref="CargoInspectionPartCategory"/> via
/// <c>VehicleDamageExtensions.ToPartCategory</c>.
/// </summary>
public enum VehicleDamageZone
{
    FrontBumper,
    Hood,
    Windshield,
    Roof,
    RearWindow,

    [Description("Trunk / Liftgate")]
    Trunk,

    RearBumper,

    LeftFrontFender,
    LeftFrontDoor,
    LeftRearDoor,
    LeftRearQuarterPanel,

    RightFrontFender,
    RightFrontDoor,
    RightRearDoor,
    RightRearQuarterPanel,

    LeftMirror,
    RightMirror,

    LeftSideGlass,
    RightSideGlass,

    LeftFrontWheel,
    LeftRearWheel,
    RightFrontWheel,
    RightRearWheel
}
