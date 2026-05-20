package com.logisticsx.driver.ui.components.inspection

import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyListScope
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.platform.LocalFocusManager
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.ui.components.capture.HandleSideEffects

/**
 * Shared scaffold for inspection screens (DVIR and cargo Condition Report).
 *
 * Wraps the common chrome both screens previously duplicated: top bar with
 * back arrow, scrollable LazyColumn with tap-to-dismiss-keyboard, snackbar
 * host wired through [HandleSideEffects], and consistent padding.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun InspectionScreenScaffold(
    title: String,
    error: String?,
    isSuccess: Boolean,
    onClearError: () -> Unit,
    onNavigateBack: () -> Unit,
    content: LazyListScope.() -> Unit
) {
    val snackbarHostState = remember { SnackbarHostState() }
    val focusManager = LocalFocusManager.current

    HandleSideEffects(
        error = error,
        isSuccess = isSuccess,
        snackbarHostState = snackbarHostState,
        onClearError = onClearError,
        onNavigateBack = onNavigateBack
    )

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(title) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                    }
                }
            )
        },
        snackbarHost = { SnackbarHost(snackbarHostState) }
    ) { padding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(16.dp)
                .pointerInput(Unit) { detectTapGestures { focusManager.clearFocus() } },
            verticalArrangement = Arrangement.spacedBy(16.dp),
            content = content
        )
    }
}
