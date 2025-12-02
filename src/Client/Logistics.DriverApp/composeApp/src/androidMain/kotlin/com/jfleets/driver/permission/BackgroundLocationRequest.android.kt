package com.jfleets.driver.permission

import android.os.Build
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.platform.LocalContext
import com.jfleets.driver.util.Logger

/**
 * Requests background location permission if:
 * - Android version is Q (10) or higher
 * - Foreground location is already granted
 * - Background location is not yet granted
 */
@Composable
actual fun RequestBackgroundLocationIfNeeded() {
    // Background location only needed on Android 10+
    if (Build.VERSION.SDK_INT < Build.VERSION_CODES.Q) {
        return
    }

    val context = LocalContext.current
    var hasRequested by remember { mutableStateOf(false) }

    // Check if foreground location is granted
    val hasForegroundLocation = context.isPermissionGranted(AppPermission.FineLocation) ||
            context.isPermissionGranted(AppPermission.CoarseLocation)

    // Check if background location is already granted
    val hasBackgroundLocation = context.isPermissionGranted(AppPermission.BackgroundLocation)

    val launcher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        Logger.d("Background location permission ${if (isGranted) "granted" else "denied"}")
    }

    LaunchedEffect(hasForegroundLocation, hasBackgroundLocation) {
        if (!hasRequested && hasForegroundLocation && !hasBackgroundLocation) {
            hasRequested = true
            Logger.d("Requesting background location permission")
            launcher.launch(AppPermission.BackgroundLocation.permission)
        }
    }
}
