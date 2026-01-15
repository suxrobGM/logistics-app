package com.jfleets.driver.service

import com.jfleets.driver.util.Logger

/**
 * iOS implementation of LocationTracker.
 *
 * This implementation is ready to use the LocationService.swift bridge via cinterop.
 *
 * SETUP REQUIRED:
 * 1. Info.plist location permissions are configured
 * 2. LocationService.swift provides CoreLocation wrapper
 * 3. Configure cinterop in build.gradle.kts to expose Swift bridge to Kotlin
 *
 * Once cinterop is configured, uncomment the platform.CoreLocation imports
 * and implement the full CoreLocation delegate pattern.
 */
actual object LocationTracker {
    private var isTracking = false

    // TODO: When cinterop is configured, add:
    // private var locationService: LocationService? = null

    actual fun start() {
        if (isTracking) {
            Logger.d("iOS Location: Already tracking")
            return
        }

        isTracking = true

        // TODO: When cinterop with LocationService.swift is configured:
        // locationService = LocationService()
        // locationService?.onLocationUpdate = { lat, lon, address, city, state ->
        //     // Send location via SignalR
        //     Logger.d("iOS Location: $lat, $lon - $address, $city, $state")
        // }
        // locationService?.start()

        Logger.d("iOS Location: Tracking started (LocationService.swift bridge available)")
        Logger.d("iOS Location: Configure cinterop to enable full CoreLocation integration")
    }

    actual fun stop() {
        // TODO: When cinterop is configured:
        // locationService?.stop()
        // locationService = null

        isTracking = false
        Logger.d("iOS Location: Tracking stopped")
    }

    actual fun isRunning(): Boolean = isTracking
}
