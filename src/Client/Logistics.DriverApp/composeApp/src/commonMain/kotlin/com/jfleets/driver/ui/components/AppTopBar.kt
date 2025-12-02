package com.jfleets.driver.ui.components

import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.WindowInsets
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable

/**
 * Shared TopAppBar component with common configuration.
 * @param title The title to display in the top app bar.
 * @param navigationIcon Composable for the navigation icon (e.g., back button).
 * @param actions Composable for action icons on the right side of the app bar.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AppTopBar(
    title: String,
    navigationIcon: @Composable () -> Unit = {},
    actions: @Composable RowScope.() -> Unit = {}
) {
    TopAppBar(
        title = { Text(title) },
        navigationIcon = navigationIcon,
        actions = actions,
        windowInsets = WindowInsets(0, 0, 0, 0)
    )
}
