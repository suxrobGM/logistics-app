package com.jfleets.driver.platform

import kotlinx.cinterop.ExperimentalForeignApi
import kotlinx.datetime.toNSDate
import platform.Foundation.NSBundle
import platform.Foundation.NSDateFormatter
import platform.Foundation.NSDateFormatterMediumStyle
import platform.Foundation.NSDateFormatterNoStyle
import platform.Foundation.NSDateFormatterShortStyle
import platform.Foundation.NSUserDefaults
import platform.UIKit.UIDevice
import kotlin.time.Instant

actual class PlatformSettings {
    private val userDefaults = NSUserDefaults.standardUserDefaults

    actual suspend fun getString(key: String): String? {
        return userDefaults.stringForKey(key)
    }

    actual suspend fun putString(key: String, value: String) {
        userDefaults.setObject(value, forKey = key)
        userDefaults.synchronize()
    }

    actual suspend fun remove(key: String) {
        userDefaults.removeObjectForKey(key)
        userDefaults.synchronize()
    }

    actual suspend fun clear() {
        val domain = NSBundle.mainBundle.bundleIdentifier
        if (domain != null) {
            userDefaults.removePersistentDomainForName(domain)
        }
    }
}

actual class LocationTracker {
    actual fun startTracking() {
        // Implementation in iOS-specific code using CLLocationManager
    }

    actual fun stopTracking() {
        // Implementation in iOS-specific code
    }

    actual suspend fun getCurrentLocation(): LocationData? {
        // Implementation in iOS-specific code
        return null
    }
}

actual class NotificationManager {
    actual fun showNotification(title: String, message: String) {
        // Implementation in iOS-specific code using UNUserNotificationCenter
    }

    actual suspend fun requestPermission(): Boolean {
        // Implementation in iOS-specific code
        return true
    }
}

actual class AuthManager {
    actual suspend fun login(): AuthResult? {
        // Implementation in iOS-specific code using AppAuth
        return null
    }

    actual suspend fun logout() {
        // Implementation in iOS-specific code
    }

    actual suspend fun getAccessToken(): String? {
        // Implementation in iOS-specific code
        return null
    }

    actual suspend fun refreshToken(refreshToken: String): AuthResult? {
        // Implementation in iOS-specific code
        return null
    }
}

actual fun getPlatformName(): String =
    UIDevice.currentDevice.systemName() + " " + UIDevice.currentDevice.systemVersion

@OptIn(ExperimentalForeignApi::class)
actual fun Instant.formatShort(): String {
    val nsDate = this.toNSDate()
    val formatter = NSDateFormatter()
    formatter.dateStyle = NSDateFormatterMediumStyle
    formatter.timeStyle = NSDateFormatterNoStyle
    return formatter.stringFromDate(nsDate)
}

@OptIn(ExperimentalForeignApi::class)
actual fun Instant.formatDateTime(): String {
    val nsDate = this.toNSDate()
    val formatter = NSDateFormatter()
    formatter.dateStyle = NSDateFormatterMediumStyle
    formatter.timeStyle = NSDateFormatterShortStyle
    return formatter.stringFromDate(nsDate)
}
