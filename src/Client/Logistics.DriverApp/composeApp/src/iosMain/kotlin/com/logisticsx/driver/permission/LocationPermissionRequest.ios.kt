package com.logisticsx.driver.permission

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect

@Composable
actual fun RequestLocationPermissionFlow(
    trigger: Boolean,
    onComplete: (granted: Boolean) -> Unit
) {
    // iOS shows the CLLocationManager prompt the first time location updates
    // start, not from a separate composable. The disclosure screen still
    // gates progression so the prompt is preceded by an explanation.
    LaunchedEffect(trigger) {
        if (trigger) {
            onComplete(true)
        }
    }
}
