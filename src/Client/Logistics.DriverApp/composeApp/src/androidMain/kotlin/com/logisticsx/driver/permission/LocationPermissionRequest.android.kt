package com.logisticsx.driver.permission

import androidx.compose.runtime.Composable

@Composable
actual fun RequestLocationPermissionFlow(
    trigger: Boolean,
    onComplete: (granted: Boolean) -> Unit
) {
    if (!trigger) return
    RequestPermissions(
        permissions = AppPermission.locationPermissions,
        onAllResults = { results ->
            onComplete(results.any { it.isGranted })
        }
    )
}
