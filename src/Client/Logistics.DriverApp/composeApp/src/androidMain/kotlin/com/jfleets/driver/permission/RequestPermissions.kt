package com.jfleets.driver.permission

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
 * Composable that handles requesting multiple permissions.
 *
 * @param permissions List of permissions to request
 * @param onAllResults Callback when all permission results are available
 */
@Composable
fun RequestPermissions(
    permissions: List<AppPermission>,
    onAllResults: (List<PermissionResult>) -> Unit = {}
) {
    val context = LocalContext.current
    var hasRequested by remember { mutableStateOf(false) }

    // Filter to only permissions that need to be requested
    val permissionsToRequest = remember(permissions) {
        val toRequest = permissions.filter { permission ->
            val shouldRequest = context.shouldRequestPermission(permission)
            Logger.d("Permission ${permission.displayName}: shouldRequest=$shouldRequest")
            shouldRequest
        }
        Logger.d("RequestPermissions: ${toRequest.size} permissions need to be requested")
        toRequest
    }

    val launcher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestMultiplePermissions()
    ) { resultsMap ->
        val results = permissions.map { permission ->
            val isGranted =
                resultsMap[permission.permission] ?: context.isPermissionGranted(permission)
            Logger.d("${permission.displayName} permission ${if (isGranted) "granted" else "denied"}")
            PermissionResult(permission, isGranted)
        }
        onAllResults(results)
    }

    LaunchedEffect(permissions) {
        if (!hasRequested) {
            hasRequested = true
            if (permissionsToRequest.isNotEmpty()) {
                Logger.d("Requesting ${permissionsToRequest.size} permissions")
                launcher.launch(permissionsToRequest.map { it.permission }.toTypedArray())
            } else {
                // All permissions already granted
                val results = permissions.map { PermissionResult(it, true) }
                onAllResults(results)
            }
        }
    }
}

/**
 * Composable that requests all startup permissions.
 * Call this from the root of your app's composition.
 *
 * @param onComplete Callback when all permission results are available
 */
@Composable
fun RequestStartupPermissions(
    onComplete: (List<PermissionResult>) -> Unit = {}
) {
    RequestPermissions(
        permissions = AppPermission.startupPermissions,
        onAllResults = onComplete
    )
}
