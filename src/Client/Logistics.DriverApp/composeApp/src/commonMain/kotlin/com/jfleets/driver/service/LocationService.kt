package com.jfleets.driver.service

/**
 * Location data class representing a geographic position.
 */
data class LocationData(
    val latitude: Double,
    val longitude: Double
)

/**
 * Expected class for getting current device location.
 * Platform-specific actual implementations handle the location retrieval.
 */
expect class LocationService {
    /**
     * Gets the current location of the device.
     * Returns null if location cannot be determined or permissions are not granted.
     */
    suspend fun getCurrentLocation(): LocationData?
}
