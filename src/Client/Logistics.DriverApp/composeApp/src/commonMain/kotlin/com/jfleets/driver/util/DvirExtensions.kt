package com.jfleets.driver.util

import com.jfleets.driver.api.models.DefectSeverity
import com.jfleets.driver.api.models.DvirInspectionCategory
import com.jfleets.driver.api.models.DvirType

val DvirType.displayName: String
    get() = when (this) {
        DvirType.PRE_TRIP -> "Pre-Trip"
        DvirType.POST_TRIP -> "Post-Trip"
    }

val DefectSeverity.displayName: String
    get() = when (this) {
        DefectSeverity.MINOR -> "Minor"
        DefectSeverity.MAJOR -> "Major"
        DefectSeverity.OUT_OF_SERVICE -> "Out of Service"
    }

val DvirInspectionCategory.displayName: String
    get() = when (this) {
        DvirInspectionCategory.AIR_COMPRESSOR -> "Air Compressor"
        DvirInspectionCategory.AIR_LINES -> "Air Lines"
        DvirInspectionCategory.BATTERY -> "Battery"
        DvirInspectionCategory.BRAKES_SERVICE -> "Service Brakes"
        DvirInspectionCategory.BRAKES_PARKING -> "Parking Brakes"
        DvirInspectionCategory.CLUTCH -> "Clutch"
        DvirInspectionCategory.COUPLING_DEVICES -> "Coupling Devices"
        DvirInspectionCategory.DEFROSTER_HEATER -> "Defroster/Heater"
        DvirInspectionCategory.DRIVE_LINE -> "Drive Line"
        DvirInspectionCategory.ENGINE -> "Engine"
        DvirInspectionCategory.EXHAUST -> "Exhaust"
        DvirInspectionCategory.FIFTH_WHEEL -> "Fifth Wheel"
        DvirInspectionCategory.FLUID_LEVELS -> "Fluid Levels"
        DvirInspectionCategory.FRAME -> "Frame"
        DvirInspectionCategory.FRONT_AXLE -> "Front Axle"
        DvirInspectionCategory.FUEL_SYSTEM -> "Fuel System"
        DvirInspectionCategory.HORN -> "Horn"
        DvirInspectionCategory.LIGHTS_HEAD -> "Headlights"
        DvirInspectionCategory.LIGHTS_TAIL -> "Tail Lights"
        DvirInspectionCategory.LIGHTS_BRAKE -> "Brake Lights"
        DvirInspectionCategory.LIGHTS_TURN -> "Turn Signals"
        DvirInspectionCategory.LIGHTS_MARKER -> "Marker Lights"
        DvirInspectionCategory.MIRRORS -> "Mirrors"
        DvirInspectionCategory.MUFFLER -> "Muffler"
        DvirInspectionCategory.OIL_PRESSURE -> "Oil Pressure"
        DvirInspectionCategory.RADIATOR -> "Radiator"
        DvirInspectionCategory.REAR_END -> "Rear End"
        DvirInspectionCategory.REFLECTORS -> "Reflectors"
        DvirInspectionCategory.SAFETY_EQUIPMENT -> "Safety Equipment"
        DvirInspectionCategory.SEAT_BELTS -> "Seat Belts"
        DvirInspectionCategory.SPEEDOMETER -> "Speedometer"
        DvirInspectionCategory.SPRINGS -> "Springs"
        DvirInspectionCategory.STARTER -> "Starter"
        DvirInspectionCategory.STEERING -> "Steering"
        DvirInspectionCategory.SUSPENSION -> "Suspension"
        DvirInspectionCategory.TIRES -> "Tires"
        DvirInspectionCategory.TRANSMISSION -> "Transmission"
        DvirInspectionCategory.TRIP_RECORDER -> "Trip Recorder"
        DvirInspectionCategory.WHEELS -> "Wheels"
        DvirInspectionCategory.WINDOWS -> "Windows"
        DvirInspectionCategory.WINDSHIELD -> "Windshield"
        DvirInspectionCategory.WIPERS -> "Wipers"
        DvirInspectionCategory.OTHER -> "Other"
    }

val DvirInspectionCategory.Companion.grouped: Map<String, List<DvirInspectionCategory>>
    get() = mapOf(
        "Brakes & Suspension" to listOf(
            DvirInspectionCategory.BRAKES_SERVICE,
            DvirInspectionCategory.BRAKES_PARKING,
            DvirInspectionCategory.SUSPENSION,
            DvirInspectionCategory.SPRINGS
        ),
        "Engine & Drivetrain" to listOf(
            DvirInspectionCategory.ENGINE,
            DvirInspectionCategory.TRANSMISSION,
            DvirInspectionCategory.CLUTCH,
            DvirInspectionCategory.DRIVE_LINE,
            DvirInspectionCategory.EXHAUST,
            DvirInspectionCategory.MUFFLER
        ),
        "Lights & Electrical" to listOf(
            DvirInspectionCategory.LIGHTS_HEAD,
            DvirInspectionCategory.LIGHTS_TAIL,
            DvirInspectionCategory.LIGHTS_BRAKE,
            DvirInspectionCategory.LIGHTS_TURN,
            DvirInspectionCategory.LIGHTS_MARKER,
            DvirInspectionCategory.BATTERY,
            DvirInspectionCategory.HORN
        ),
        "Tires & Wheels" to listOf(
            DvirInspectionCategory.TIRES,
            DvirInspectionCategory.WHEELS,
            DvirInspectionCategory.FRONT_AXLE,
            DvirInspectionCategory.REAR_END
        ),
        "Air System" to listOf(
            DvirInspectionCategory.AIR_COMPRESSOR,
            DvirInspectionCategory.AIR_LINES
        ),
        "Fluids & Cooling" to listOf(
            DvirInspectionCategory.FLUID_LEVELS,
            DvirInspectionCategory.OIL_PRESSURE,
            DvirInspectionCategory.RADIATOR,
            DvirInspectionCategory.FUEL_SYSTEM
        ),
        "Cab & Body" to listOf(
            DvirInspectionCategory.MIRRORS,
            DvirInspectionCategory.WINDOWS,
            DvirInspectionCategory.WINDSHIELD,
            DvirInspectionCategory.WIPERS,
            DvirInspectionCategory.DEFROSTER_HEATER,
            DvirInspectionCategory.SEAT_BELTS
        ),
        "Coupling & Frame" to listOf(
            DvirInspectionCategory.COUPLING_DEVICES,
            DvirInspectionCategory.FIFTH_WHEEL,
            DvirInspectionCategory.FRAME,
            DvirInspectionCategory.REFLECTORS
        ),
        "Safety & Other" to listOf(
            DvirInspectionCategory.SAFETY_EQUIPMENT,
            DvirInspectionCategory.STEERING,
            DvirInspectionCategory.STARTER,
            DvirInspectionCategory.SPEEDOMETER,
            DvirInspectionCategory.TRIP_RECORDER,
            DvirInspectionCategory.OTHER
        )
    )
