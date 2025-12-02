package com.jfleets.driver.permission

import android.Manifest
import android.os.Build

/**
 * Represents a permission that can be requested at runtime.
 * Add new permissions here as needed.
 *
 * @param permission The Android permission string.
 * @param minSdkVersion The minimum SDK version required for this permission. Defaults to 1.
 * @param displayName A user-friendly name for the permission.
 */
sealed class AppPermission(
    val permission: String,
    val minSdkVersion: Int = 1,
    val displayName: String
) {
    /** Required for push notifications on Android 13+ */
    data object PostNotifications : AppPermission(
        permission = Manifest.permission.POST_NOTIFICATIONS,
        minSdkVersion = Build.VERSION_CODES.TIRAMISU,
        displayName = "Notifications"
    )

    /** Fine location for GPS tracking */
    data object FineLocation : AppPermission(
        permission = Manifest.permission.ACCESS_FINE_LOCATION,
        minSdkVersion = 1,
        displayName = "Location"
    )

    /** Coarse location as fallback */
    data object CoarseLocation : AppPermission(
        permission = Manifest.permission.ACCESS_COARSE_LOCATION,
        minSdkVersion = 1,
        displayName = "Approximate Location"
    )

    /** Background location for tracking while app is in background */
    data object BackgroundLocation : AppPermission(
        permission = Manifest.permission.ACCESS_BACKGROUND_LOCATION,
        minSdkVersion = Build.VERSION_CODES.Q,
        displayName = "Background Location"
    )

    /** Camera for document scanning */
    data object Camera : AppPermission(
        permission = Manifest.permission.CAMERA,
        minSdkVersion = 1,
        displayName = "Camera"
    )

    companion object {
        /**
         * Permissions to request at app startup.
         * Note: BackgroundLocation must be requested separately AFTER foreground location is granted.
         * Android silently denies all permissions if background location is requested with others.
         */
        val startupPermissions: List<AppPermission> by lazy {
            listOf(
                PostNotifications,
                FineLocation,
                CoarseLocation,
                Camera
            )
        }

        /** All defined permissions */
        val all: List<AppPermission> by lazy {
            listOf(
                PostNotifications,
                FineLocation,
                CoarseLocation,
                BackgroundLocation,
                Camera
            )
        }
    }
}
