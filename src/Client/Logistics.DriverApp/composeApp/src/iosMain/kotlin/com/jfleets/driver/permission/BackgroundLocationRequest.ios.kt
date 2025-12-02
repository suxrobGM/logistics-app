package com.jfleets.driver.permission

import androidx.compose.runtime.Composable

/**
 * iOS implementation - background location is handled differently on iOS
 * through Info.plist configuration and CLLocationManager.
 */
@Composable
actual fun RequestBackgroundLocationIfNeeded() {
    // No-op on iOS - background location is configured via Info.plist
    // and requested through CLLocationManager when location updates are started
}
