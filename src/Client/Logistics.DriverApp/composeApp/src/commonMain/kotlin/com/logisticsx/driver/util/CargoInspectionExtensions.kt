package com.logisticsx.driver.util

import com.logisticsx.driver.api.models.CargoInspectionPartCategory
import com.logisticsx.driver.api.models.LoadType

/**
 * Driver-side display name for each [CargoInspectionPartCategory]. Backed
 * server-side by `[Description]` attributes on the enum — duplicated here so
 * the driver UI can render display names without an extra round-trip.
 */
val CargoInspectionPartCategory.displayName: String
    get() = when (this) {
        // Vehicle cargo
        CargoInspectionPartCategory.VEHICLE_FRONT_BUMPER -> "Front Bumper"
        CargoInspectionPartCategory.VEHICLE_REAR_BUMPER -> "Rear Bumper"
        CargoInspectionPartCategory.VEHICLE_HOOD -> "Hood"
        CargoInspectionPartCategory.VEHICLE_ROOF -> "Roof"
        CargoInspectionPartCategory.VEHICLE_TRUNK_LIFTGATE -> "Trunk / Liftgate"
        CargoInspectionPartCategory.VEHICLE_FRONT_LEFT_DOOR -> "Front Left Door"
        CargoInspectionPartCategory.VEHICLE_FRONT_RIGHT_DOOR -> "Front Right Door"
        CargoInspectionPartCategory.VEHICLE_REAR_LEFT_DOOR -> "Rear Left Door"
        CargoInspectionPartCategory.VEHICLE_REAR_RIGHT_DOOR -> "Rear Right Door"
        CargoInspectionPartCategory.VEHICLE_FENDERS -> "Fenders"
        CargoInspectionPartCategory.VEHICLE_WHEELS -> "Wheels"
        CargoInspectionPartCategory.VEHICLE_MIRRORS -> "Mirrors"
        CargoInspectionPartCategory.VEHICLE_WINDSHIELD -> "Windshield"
        CargoInspectionPartCategory.VEHICLE_SIDE_GLASS -> "Side Glass"
        CargoInspectionPartCategory.VEHICLE_LIGHTS -> "Lights"
        CargoInspectionPartCategory.VEHICLE_BODY_PANELS -> "Body Panels"
        CargoInspectionPartCategory.VEHICLE_INTERIOR -> "Interior"

        // Container cargo
        CargoInspectionPartCategory.CONTAINER_FRONT_WALL -> "Front Wall"
        CargoInspectionPartCategory.CONTAINER_REAR_DOORS -> "Rear Doors"
        CargoInspectionPartCategory.CONTAINER_LEFT_WALL -> "Left Wall"
        CargoInspectionPartCategory.CONTAINER_RIGHT_WALL -> "Right Wall"
        CargoInspectionPartCategory.CONTAINER_ROOF -> "Roof"
        CargoInspectionPartCategory.CONTAINER_FLOOR -> "Floor"
        CargoInspectionPartCategory.CONTAINER_LOCKING_HARDWARE -> "Locking Rods / Hinges"
        CargoInspectionPartCategory.CONTAINER_CORNER_CASTINGS -> "Corner Castings"
        CargoInspectionPartCategory.CONTAINER_SEAL -> "Seal"

        // Generic freight
        CargoInspectionPartCategory.GENERIC_WALLS -> "Walls"
        CargoInspectionPartCategory.GENERIC_DOORS -> "Doors"
        CargoInspectionPartCategory.GENERIC_FLOOR -> "Floor"
        CargoInspectionPartCategory.GENERIC_ROOF -> "Roof"
        CargoInspectionPartCategory.GENERIC_LIGHTING -> "Lighting"
        CargoInspectionPartCategory.GENERIC_TIRES -> "Tires"
        CargoInspectionPartCategory.GENERIC_SECUREMENT -> "Strapping / Securement"

        CargoInspectionPartCategory.OTHER -> "Other"
    }

/**
 * Returns the catalog grouped into expandable sections for the given cargo type.
 * Drives [com.logisticsx.driver.ui.components.inspection.AddDefectDialog].
 */
fun LoadType.cargoPartCatalogGrouped(): Map<String, List<CargoInspectionPartCategory>> =
    when (this) {
        LoadType.VEHICLE -> mapOf(
            "Exterior - Front" to listOf(
                CargoInspectionPartCategory.VEHICLE_FRONT_BUMPER,
                CargoInspectionPartCategory.VEHICLE_HOOD,
                CargoInspectionPartCategory.VEHICLE_WINDSHIELD,
                CargoInspectionPartCategory.VEHICLE_LIGHTS
            ),
            "Exterior - Rear" to listOf(
                CargoInspectionPartCategory.VEHICLE_REAR_BUMPER,
                CargoInspectionPartCategory.VEHICLE_TRUNK_LIFTGATE
            ),
            "Doors" to listOf(
                CargoInspectionPartCategory.VEHICLE_FRONT_LEFT_DOOR,
                CargoInspectionPartCategory.VEHICLE_FRONT_RIGHT_DOOR,
                CargoInspectionPartCategory.VEHICLE_REAR_LEFT_DOOR,
                CargoInspectionPartCategory.VEHICLE_REAR_RIGHT_DOOR
            ),
            "Body & Glass" to listOf(
                CargoInspectionPartCategory.VEHICLE_FENDERS,
                CargoInspectionPartCategory.VEHICLE_ROOF,
                CargoInspectionPartCategory.VEHICLE_BODY_PANELS,
                CargoInspectionPartCategory.VEHICLE_SIDE_GLASS,
                CargoInspectionPartCategory.VEHICLE_MIRRORS
            ),
            "Wheels & Interior" to listOf(
                CargoInspectionPartCategory.VEHICLE_WHEELS,
                CargoInspectionPartCategory.VEHICLE_INTERIOR
            ),
            "Other" to listOf(CargoInspectionPartCategory.OTHER)
        )

        LoadType.INTERMODAL_CONTAINER,
        LoadType.TANK_CONTAINER,
        LoadType.REEFER_CONTAINER -> mapOf(
            "Walls" to listOf(
                CargoInspectionPartCategory.CONTAINER_FRONT_WALL,
                CargoInspectionPartCategory.CONTAINER_LEFT_WALL,
                CargoInspectionPartCategory.CONTAINER_RIGHT_WALL
            ),
            "Top & Bottom" to listOf(
                CargoInspectionPartCategory.CONTAINER_ROOF,
                CargoInspectionPartCategory.CONTAINER_FLOOR
            ),
            "Doors & Hardware" to listOf(
                CargoInspectionPartCategory.CONTAINER_REAR_DOORS,
                CargoInspectionPartCategory.CONTAINER_LOCKING_HARDWARE,
                CargoInspectionPartCategory.CONTAINER_CORNER_CASTINGS
            ),
            "Seal" to listOf(CargoInspectionPartCategory.CONTAINER_SEAL),
            "Other" to listOf(CargoInspectionPartCategory.OTHER)
        )

        else -> mapOf(
            "Trailer Body" to listOf(
                CargoInspectionPartCategory.GENERIC_WALLS,
                CargoInspectionPartCategory.GENERIC_DOORS,
                CargoInspectionPartCategory.GENERIC_FLOOR,
                CargoInspectionPartCategory.GENERIC_ROOF
            ),
            "Equipment" to listOf(
                CargoInspectionPartCategory.GENERIC_LIGHTING,
                CargoInspectionPartCategory.GENERIC_TIRES,
                CargoInspectionPartCategory.GENERIC_SECUREMENT
            ),
            "Other" to listOf(CargoInspectionPartCategory.OTHER)
        )
    }

/** True for any container cargo type. */
val LoadType.isContainerLoad: Boolean
    get() = this == LoadType.INTERMODAL_CONTAINER
        || this == LoadType.TANK_CONTAINER
        || this == LoadType.REEFER_CONTAINER
