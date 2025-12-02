package com.jfleets.driver.permission

import androidx.compose.runtime.Composable

/**
 * Composable that requests background location permission if foreground location is already granted.
 * On Android, this shows a system dialog. On iOS, this is a no-op (handled differently).
 */
@Composable
expect fun RequestBackgroundLocationIfNeeded()
