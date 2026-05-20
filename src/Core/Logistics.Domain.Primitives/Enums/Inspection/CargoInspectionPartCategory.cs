using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Standardized cargo-inspection part categories used when documenting damage
/// found during pickup/delivery inspections. The valid subset is determined by
/// the <see cref="LoadType"/> of the load being inspected — see
/// <c>CargoInspectionPartCategoryExtensions.GetCatalogFor</c>.
/// </summary>
public enum CargoInspectionPartCategory
{
    // ── Vehicle cargo (auto-haul) ────────────────────────────────────────
    [Description("Front Bumper")] VehicleFrontBumper,
    [Description("Rear Bumper")]  VehicleRearBumper,
    VehicleHood,
    VehicleRoof,

    [Description("Trunk / Liftgate")] VehicleTrunkLiftgate,

    [Description("Front Left Door")]  VehicleFrontLeftDoor,
    [Description("Front Right Door")] VehicleFrontRightDoor,
    [Description("Rear Left Door")]   VehicleRearLeftDoor,
    [Description("Rear Right Door")]  VehicleRearRightDoor,

    VehicleFenders,
    VehicleWheels,
    VehicleMirrors,
    VehicleWindshield,

    [Description("Side Glass")] VehicleSideGlass,

    VehicleLights,

    [Description("Body Panels")] VehicleBodyPanels,

    VehicleInterior,

    // ── Container cargo (ISO 6346 parts) ─────────────────────────────────
    [Description("Front Wall")] ContainerFrontWall,
    [Description("Rear Doors")] ContainerRearDoors,
    [Description("Left Wall")]  ContainerLeftWall,
    [Description("Right Wall")] ContainerRightWall,

    ContainerRoof,
    ContainerFloor,

    [Description("Locking Rods / Hinges")] ContainerLockingHardware,
    [Description("Corner Castings")]       ContainerCornerCastings,

    ContainerSeal,

    // ── Generic freight / trailer fallback ───────────────────────────────
    GenericWalls,
    GenericDoors,
    GenericFloor,
    GenericRoof,
    GenericLighting,
    GenericTires,

    [Description("Strapping / Securement")] GenericSecurement,

    Other
}
