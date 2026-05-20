package com.logisticsx.driver.ui.components

import androidx.compose.runtime.Composable
import com.logisticsx.driver.viewmodel.base.UiState

/**
 * Renders the standard Loading → Error → Success ladder for a [UiState], removing the
 * per-screen `when` + unchecked-cast boilerplate. The [content] block receives the
 * already-typed success data.
 */
@Composable
fun <T> UiStateContent(
    state: UiState<T>,
    onRetry: () -> Unit,
    content: @Composable (T) -> Unit
) {
    when (state) {
        is UiState.Loading -> LoadingIndicator()
        is UiState.Error -> ErrorView(message = state.message, onRetry = onRetry)
        is UiState.Success -> content(state.data)
    }
}
