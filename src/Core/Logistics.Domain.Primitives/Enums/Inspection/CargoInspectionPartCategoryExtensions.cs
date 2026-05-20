namespace Logistics.Domain.Primitives.Enums;

public static class CargoInspectionPartCategoryExtensions
{
    private static readonly CargoInspectionPartCategory[] VehicleParts =
    [
        CargoInspectionPartCategory.VehicleFrontBumper,
        CargoInspectionPartCategory.VehicleRearBumper,
        CargoInspectionPartCategory.VehicleHood,
        CargoInspectionPartCategory.VehicleRoof,
        CargoInspectionPartCategory.VehicleTrunkLiftgate,
        CargoInspectionPartCategory.VehicleFrontLeftDoor,
        CargoInspectionPartCategory.VehicleFrontRightDoor,
        CargoInspectionPartCategory.VehicleRearLeftDoor,
        CargoInspectionPartCategory.VehicleRearRightDoor,
        CargoInspectionPartCategory.VehicleFenders,
        CargoInspectionPartCategory.VehicleWheels,
        CargoInspectionPartCategory.VehicleMirrors,
        CargoInspectionPartCategory.VehicleWindshield,
        CargoInspectionPartCategory.VehicleSideGlass,
        CargoInspectionPartCategory.VehicleLights,
        CargoInspectionPartCategory.VehicleBodyPanels,
        CargoInspectionPartCategory.VehicleInterior,
        CargoInspectionPartCategory.Other
    ];

    private static readonly CargoInspectionPartCategory[] ContainerParts =
    [
        CargoInspectionPartCategory.ContainerFrontWall,
        CargoInspectionPartCategory.ContainerRearDoors,
        CargoInspectionPartCategory.ContainerLeftWall,
        CargoInspectionPartCategory.ContainerRightWall,
        CargoInspectionPartCategory.ContainerRoof,
        CargoInspectionPartCategory.ContainerFloor,
        CargoInspectionPartCategory.ContainerLockingHardware,
        CargoInspectionPartCategory.ContainerCornerCastings,
        CargoInspectionPartCategory.ContainerSeal,
        CargoInspectionPartCategory.Other
    ];

    private static readonly CargoInspectionPartCategory[] GenericParts =
    [
        CargoInspectionPartCategory.GenericWalls,
        CargoInspectionPartCategory.GenericDoors,
        CargoInspectionPartCategory.GenericFloor,
        CargoInspectionPartCategory.GenericRoof,
        CargoInspectionPartCategory.GenericLighting,
        CargoInspectionPartCategory.GenericTires,
        CargoInspectionPartCategory.GenericSecurement,
        CargoInspectionPartCategory.Other
    ];

    /// <summary>
    /// Returns the subset of part categories valid for the given <paramref name="loadType"/>.
    /// Returned arrays are shared singletons — callers should treat them as read-only.
    /// </summary>
    public static CargoInspectionPartCategory[] GetCatalogFor(LoadType loadType) =>
        loadType switch
        {
            LoadType.Vehicle => VehicleParts,

            LoadType.IntermodalContainer
                or LoadType.TankContainer
                or LoadType.ReeferContainer => ContainerParts,

            _ => GenericParts
        };

    /// <summary>
    /// True when <paramref name="part"/> is allowed for the given <paramref name="loadType"/>.
    /// </summary>
    public static bool IsValidFor(this CargoInspectionPartCategory part, LoadType loadType) =>
        GetCatalogFor(loadType).Contains(part);

    /// <summary>
    /// True when the load type is one of the container variants.
    /// </summary>
    public static bool IsContainerLoad(this LoadType loadType) =>
        loadType is LoadType.IntermodalContainer
            or LoadType.TankContainer
            or LoadType.ReeferContainer;
}
