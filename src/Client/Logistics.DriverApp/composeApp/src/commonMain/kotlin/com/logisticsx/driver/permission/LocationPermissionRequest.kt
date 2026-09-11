package com.logisticsx.driver.permission

import androidx.compose.runtime.Composable

/**
 * Location-permission requester used by the disclosure screen.
 *
 * Android shows the runtime prompt for fine and coarse location and reports whether either was
 * granted. iOS reports true immediately: its own prompt appears later, when the tracker starts.
 *
 * Set [trigger] to true to request once, then reset it to false when [onComplete] fires.
 */
@Composable
expect fun RequestLocationPermissionFlow(
    trigger: Boolean,
    onComplete: (granted: Boolean) -> Unit
)
