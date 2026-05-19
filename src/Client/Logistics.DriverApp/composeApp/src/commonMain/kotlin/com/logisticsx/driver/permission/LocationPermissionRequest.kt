package com.logisticsx.driver.permission

import androidx.compose.runtime.Composable

/**
 * Cross-platform location-permission requester used by the disclosure screen.
 *
 * On Android this fires the OS runtime prompt for `ACCESS_FINE_LOCATION` and
 * `ACCESS_COARSE_LOCATION` and invokes [onComplete] with whether either was
 * granted.
 *
 * On iOS this is a no-op that immediately invokes `onComplete(true)`; the
 * actual `CLLocationManager` authorization prompt is shown later, when
 * `DutyStatusManager.goOnDuty()` starts the tracker.
 *
 * @param trigger Set to true to launch the permission request once. After the
 *   request completes the caller should reset this back to false.
 */
@Composable
expect fun RequestLocationPermissionFlow(
    trigger: Boolean,
    onComplete: (granted: Boolean) -> Unit
)
