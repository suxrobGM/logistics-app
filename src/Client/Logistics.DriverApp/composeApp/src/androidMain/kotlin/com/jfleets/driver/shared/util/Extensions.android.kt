package com.jfleets.driver.shared.util

import android.location.Location

/**
 * Android-specific extension functions
 */

// Location distance calculation
fun Location.distanceTo(lat: Double, lon: Double): Float {
    val targetLocation = Location("").apply {
        latitude = lat
        longitude = lon
    }
    return this.distanceTo(targetLocation)
}

fun calculateDistance(lat1: Double, lon1: Double, lat2: Double, lon2: Double): Float {
    val startLocation = Location("").apply {
        latitude = lat1
        longitude = lon1
    }
    val endLocation = Location("").apply {
        latitude = lat2
        longitude = lon2
    }
    return startLocation.distanceTo(endLocation)
}
