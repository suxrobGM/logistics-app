package com.jfleets.driver.permission

import android.content.Context
import android.content.pm.PackageManager
import android.os.Build
import androidx.core.content.ContextCompat

/**
 * Checks if a permission is granted.
 * @param permission The permission to check.
 * @return True if the permission is granted, false otherwise.
 */
fun Context.isPermissionGranted(permission: AppPermission): Boolean {
    // Skip check if below minimum SDK version (permission not needed)
    if (Build.VERSION.SDK_INT < permission.minSdkVersion) {
        return true
    }
    return ContextCompat.checkSelfPermission(
        this,
        permission.permission
    ) == PackageManager.PERMISSION_GRANTED
}

/**
 * Checks if a permission should be requested (not granted and meets SDK requirements).
 * @param permission The permission to check.
 * @return True if the permission should be requested, false otherwise.
 */
fun Context.shouldRequestPermission(permission: AppPermission): Boolean {
    if (Build.VERSION.SDK_INT < permission.minSdkVersion) {
        return false
    }
    return !isPermissionGranted(permission)
}
