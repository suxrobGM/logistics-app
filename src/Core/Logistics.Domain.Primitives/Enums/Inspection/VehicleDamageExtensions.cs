namespace Logistics.Domain.Primitives.Enums;

public static class VehicleDamageExtensions
{
    /// <summary>
    /// The code written on car hauler BOL diagrams: S scratch, D dent, C chip, CR crack, M missing, B broken.
    /// </summary>
    public static string GetCode(this VehicleDamageType type) => type switch
    {
        VehicleDamageType.Scratch => "S",
        VehicleDamageType.Dent => "D",
        VehicleDamageType.Chip => "C",
        VehicleDamageType.Crack => "CR",
        VehicleDamageType.Missing => "M",
        VehicleDamageType.Broken => "B",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    /// <summary>
    /// The part category a diagram zone belongs to, so zone-marked defects still group and filter
    /// with the defects recorded through the part list.
    /// </summary>
    public static CargoInspectionPartCategory ToPartCategory(this VehicleDamageZone zone) => zone switch
    {
        VehicleDamageZone.FrontBumper => CargoInspectionPartCategory.VehicleFrontBumper,
        VehicleDamageZone.RearBumper => CargoInspectionPartCategory.VehicleRearBumper,
        VehicleDamageZone.Hood => CargoInspectionPartCategory.VehicleHood,
        VehicleDamageZone.Roof => CargoInspectionPartCategory.VehicleRoof,
        VehicleDamageZone.Trunk => CargoInspectionPartCategory.VehicleTrunkLiftgate,
        VehicleDamageZone.Windshield => CargoInspectionPartCategory.VehicleWindshield,
        VehicleDamageZone.RearWindow
            or VehicleDamageZone.LeftSideGlass
            or VehicleDamageZone.RightSideGlass => CargoInspectionPartCategory.VehicleSideGlass,
        VehicleDamageZone.LeftFrontDoor => CargoInspectionPartCategory.VehicleFrontLeftDoor,
        VehicleDamageZone.RightFrontDoor => CargoInspectionPartCategory.VehicleFrontRightDoor,
        VehicleDamageZone.LeftRearDoor => CargoInspectionPartCategory.VehicleRearLeftDoor,
        VehicleDamageZone.RightRearDoor => CargoInspectionPartCategory.VehicleRearRightDoor,
        VehicleDamageZone.LeftFrontFender
            or VehicleDamageZone.RightFrontFender => CargoInspectionPartCategory.VehicleFenders,
        VehicleDamageZone.LeftRearQuarterPanel
            or VehicleDamageZone.RightRearQuarterPanel => CargoInspectionPartCategory.VehicleBodyPanels,
        VehicleDamageZone.LeftMirror
            or VehicleDamageZone.RightMirror => CargoInspectionPartCategory.VehicleMirrors,
        VehicleDamageZone.LeftFrontWheel
            or VehicleDamageZone.LeftRearWheel
            or VehicleDamageZone.RightFrontWheel
            or VehicleDamageZone.RightRearWheel => CargoInspectionPartCategory.VehicleWheels,
        _ => throw new ArgumentOutOfRangeException(nameof(zone), zone, null)
    };
}
