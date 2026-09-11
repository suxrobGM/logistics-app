package com.logisticsx.driver.permission

import android.Manifest
import android.os.Build
import androidx.annotation.RequiresApi

/**
 * A runtime permission this app can request.
 *
 * [minSdkVersion] defaults to 1, so set it only for permissions introduced in a later release.
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
        displayName = "Location"
    )

    /** Coarse location as fallback */
    data object CoarseLocation : AppPermission(
        permission = Manifest.permission.ACCESS_COARSE_LOCATION,
        displayName = "Approximate Location"
    )

    /** Camera for document scanning */
    data object Camera : AppPermission(
        permission = Manifest.permission.CAMERA,
        displayName = "Camera"
    )

    companion object {
        /**
         * Permissions requested at app startup. Location is NOT included - it
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
    }
}
