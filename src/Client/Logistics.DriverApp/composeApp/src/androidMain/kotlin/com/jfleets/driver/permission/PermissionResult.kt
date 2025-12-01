package com.jfleets.driver.permission

/**
 * Result of a permission request.
 *
 * @param permission The permission that was requested.
 * @param isGranted Whether the permission was granted.
 */
data class PermissionResult(
    val permission: AppPermission,
    val isGranted: Boolean
)
