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
 * Composable that handles requesting a single permission.
 *
 * @param permission The permission to request
 * @param onResult Callback with the result of the permission request
 */
@Composable
fun RequestPermission(
    permission: AppPermission,
    onResult: (PermissionResult) -> Unit = {}
) {
    val context = LocalContext.current
    var hasRequested by remember { mutableStateOf(false) }

    val launcher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        Logger.d("${permission.displayName} permission ${if (isGranted) "granted" else "denied"}")
        onResult(PermissionResult(permission, isGranted))
    }

    LaunchedEffect(permission) {
        if (!hasRequested && context.shouldRequestPermission(permission)) {
            hasRequested = true
            Logger.d("Requesting ${permission.displayName} permission")
            launcher.launch(permission.permission)
        } else if (!hasRequested && context.isPermissionGranted(permission)) {
            hasRequested = true
            Logger.d("${permission.displayName} permission already granted")
            onResult(PermissionResult(permission, true))
        }
    }
}
