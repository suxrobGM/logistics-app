package com.logisticsx.driver.permission

/** Result of a permission request. */
data class PermissionResult(
    val permission: AppPermission,
    val isGranted: Boolean
)
