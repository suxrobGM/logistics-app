package com.jfleets.driver.service

import android.content.Context
import android.content.Intent
import com.jfleets.driver.util.Logger
import org.koin.core.component.KoinComponent
import org.koin.core.component.inject

actual object LocationTracker : KoinComponent {
    private val context: Context by inject()
    private var isServiceRunning = false

    // Service class name in the app module (resolved via reflection)
    private const val SERVICE_CLASS_NAME = "com.jfleets.driver.service.LocationTrackingService"

    actual fun start() {
        if (isServiceRunning) {
            Logger.d("Location tracking already running")
            return
        }

        try {
            val serviceClass = Class.forName(SERVICE_CLASS_NAME)
            val intent = Intent(context, serviceClass)
            context.startForegroundService(intent)
            isServiceRunning = true
            Logger.d("Location tracking service started")
        } catch (e: Exception) {
            Logger.e("Failed to start location tracking service", e)
        }
    }

    actual fun stop() {
        try {
            val serviceClass = Class.forName(SERVICE_CLASS_NAME)
            val intent = Intent(context, serviceClass)
            context.stopService(intent)
            isServiceRunning = false
            Logger.d("Location tracking service stopped")
        } catch (e: Exception) {
            Logger.e("Failed to stop location tracking service", e)
        }
    }

    actual fun isRunning(): Boolean = isServiceRunning
}
