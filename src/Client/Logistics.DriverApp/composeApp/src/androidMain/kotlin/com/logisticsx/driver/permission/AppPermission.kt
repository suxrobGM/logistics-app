package com.logisticsx.driver.permission

import android.Manifest
import android.os.Build
import androidx.annotation.RequiresApi

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
    @RequiresApi(Build.VERSION_CODES.TIRAMISU)
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

    /** Camera for document scanning */
    data object Camera : AppPermission(
        permission = Manifest.permission.CAMERA,
        minSdkVersion = 1,
        displayName = "Camera"
    )

    companion object {
        /**
         * Permissions requested at app startup. Location is NOT included — it
         * is requested on-demand from the location disclosure screen so the
         * driver sees the rationale before the OS prompt (Play policy).
         */
        val startupPermissions: List<AppPermission> by lazy {
            val permissions = mutableListOf<AppPermission>(Camera)
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                permissions.add(PostNotifications)
            }
            permissions
        }

        /** Foreground location permissions, requested from the disclosure screen. */
        val locationPermissions: List<AppPermission> by lazy {
            listOf(FineLocation, CoarseLocation)
        }

        /** All defined permissions */
        val all: List<AppPermission> by lazy {
            val permissions = mutableListOf(
                FineLocation,
                CoarseLocation,
                Camera
            )

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                permissions.add(PostNotifications)
            }
            permissions
        }
    }
}
