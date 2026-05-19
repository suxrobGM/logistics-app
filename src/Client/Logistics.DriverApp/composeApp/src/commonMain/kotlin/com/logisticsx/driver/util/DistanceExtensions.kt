package com.logisticsx.driver.util

import com.logisticsx.driver.model.DistanceUnit
import kotlin.math.PI
import kotlin.math.atan2
import kotlin.math.cos
import kotlin.math.sin
import kotlin.math.sqrt

private const val EARTH_RADIUS_METERS = 6_371_000.0

/**
 * Great-circle distance in meters between two coordinates (Haversine).
 * commonMain has no platform `Location.distanceTo`, so this is the shared helper
 * for any consumer that needs radius checks in shared code.
 */
fun distanceMeters(lat1: Double, lon1: Double, lat2: Double, lon2: Double): Double {
    val phi1 = lat1.toRadians()
    val phi2 = lat2.toRadians()
    val deltaPhi = (lat2 - lat1).toRadians()
    val deltaLambda = (lon2 - lon1).toRadians()

    val a = sin(deltaPhi / 2).let { it * it } +
        cos(phi1) * cos(phi2) * sin(deltaLambda / 2).let { it * it }
    val c = 2 * atan2(sqrt(a), sqrt(1 - a))
    return EARTH_RADIUS_METERS * c
}

private fun Double.toRadians(): Double = this * PI / 180.0

/**
 * Converts a distance in meters to miles.
 */
fun Double.toMiles(): Double = this * 0.000621371

/**
 * Converts a distance in meters to kilometers.
 */
fun Double.toKilometers(): Double = this / 1000.0

/**
 * Formats a distance in meters to a string in miles with one decimal place.
 * Example: 1609.34 meters -> "1.0 mi"
 */
fun Double.formatMiDistance(): String {
    return formatDistance(DistanceUnit.MILES)
}

fun Double.formatKmDistance(): String {
    return formatDistance(DistanceUnit.KILOMETERS)
}

/**
 * Formats a distance in meters to a string based on the specified unit.
 * The actual implementation is platform-specific.
 * @param unit The distance unit to format the distance in.
 * @return The formatted distance string.
 */
expect fun Double.formatDistance(unit: DistanceUnit): String
