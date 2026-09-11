package com.logisticsx.driver.permission

import android.content.Context
import android.content.pm.PackageManager
import android.os.Build
import androidx.core.content.ContextCompat

/** Whether [permission] is granted, treating it as granted below its minimum SDK version. */
fun Context.isPermissionGranted(permission: AppPermission): Boolean {
    if (Build.VERSION.SDK_INT < permission.minSdkVersion) {
        return true
    }
    return ContextCompat.checkSelfPermission(
        this,
        permission.permission
    ) == PackageManager.PERMISSION_GRANTED
}

/** Whether [permission] still needs requesting: not granted, and required on this SDK version. */
fun Context.shouldRequestPermission(permission: AppPermission): Boolean =
    !isPermissionGranted(permission)
