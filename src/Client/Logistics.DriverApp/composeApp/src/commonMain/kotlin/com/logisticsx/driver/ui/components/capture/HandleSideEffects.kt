package com.logisticsx.driver.ui.components.capture

import androidx.compose.material3.SnackbarHostState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect

/**
 * Shared side-effect handler for capture form screens (DVIR, POD, Condition Report).
 * Shows errors via snackbar and navigates back on success.
 */
@Composable
fun HandleSideEffects(
    error: String?,
    isSuccess: Boolean,
    snackbarHostState: SnackbarHostState,
    onClearError: () -> Unit,
    onNavigateBack: () -> Unit
) {
    LaunchedEffect(error) {
        error?.let {
            snackbarHostState.showSnackbar(it)
            onClearError()
        }
    }

    LaunchedEffect(isSuccess) {
        if (isSuccess) {
            onNavigateBack()
        }
    }
}
