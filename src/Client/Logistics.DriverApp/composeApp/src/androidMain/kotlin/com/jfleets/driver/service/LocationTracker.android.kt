package com.jfleets.driver.service

import android.content.Context
import android.content.Intent
import android.os.Build
import com.jfleets.driver.util.Logger
import org.koin.core.component.KoinComponent
import org.koin.core.component.inject

actual object LocationTracker : KoinComponent {
    private val context: Context by inject()
    private var isServiceRunning = false

    actual fun start() {
        if (isServiceRunning) {
            Logger.d("Location tracking already running")
            return
        }

        try {
            val intent = Intent(context, LocationTrackingService::class.java)
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                context.startForegroundService(intent)
            } else {
                context.startService(intent)
            }
            isServiceRunning = true
            Logger.d("Location tracking service started")
        } catch (e: Exception) {
            Logger.e("Failed to start location tracking service", e)
        }
    }

    actual fun stop() {
        try {
            val intent = Intent(context, LocationTrackingService::class.java)
            context.stopService(intent)
            isServiceRunning = false
            Logger.d("Location tracking service stopped")
        } catch (e: Exception) {
            Logger.e("Failed to stop location tracking service", e)
        }
    }

    actual fun isRunning(): Boolean = isServiceRunning
}
