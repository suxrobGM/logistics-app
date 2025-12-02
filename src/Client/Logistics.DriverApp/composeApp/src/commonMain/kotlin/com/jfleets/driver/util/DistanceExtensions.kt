package com.jfleets.driver.util

import com.jfleets.driver.model.DistanceUnit

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
