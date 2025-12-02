package com.jfleets.driver.service

/**
 * Platform-specific location tracking service starter.
 * - Android: Starts the LocationTrackingService foreground service
 * - iOS: Starts Core Location background updates
 */
expect object LocationTracker {
    /**
     * Starts the location tracking service.
     * Should be called when user is logged in and location permissions are granted.
     */
    fun start()

    /**
     * Stops the location tracking service.
     */
    fun stop()

    /**
     * Checks if location tracking is currently running.
     */
    fun isRunning(): Boolean
}
