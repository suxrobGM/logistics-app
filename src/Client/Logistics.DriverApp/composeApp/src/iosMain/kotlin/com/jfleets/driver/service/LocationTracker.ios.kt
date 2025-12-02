package com.jfleets.driver.service

import com.jfleets.driver.util.Logger

actual object LocationTracker {
    private var isTracking = false

    actual fun start() {
        // TODO: Implement iOS location tracking using Core Location
        Logger.w("iOS location tracking not yet implemented")
        isTracking = true
    }

    actual fun stop() {
        Logger.w("iOS location tracking not yet implemented")
        isTracking = false
    }

    actual fun isRunning(): Boolean = isTracking
}
