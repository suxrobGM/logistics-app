package com.logisticsx.driver.viewmodel.base

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.logisticsx.driver.util.Logger
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

/**
 * Base ViewModel providing consistent async patterns.
 * All ViewModels should extend this instead of ViewModel() directly.
 */
abstract class BaseViewModel : ViewModel() {

    /**
     * Launch a suspend block that updates a [UiState] flow automatically:
     * sets Loading before execution, Success on completion, Error on failure.
     */
    protected fun <T> launchWithState(
        stateFlow: MutableStateFlow<UiState<T>>,
        block: suspend () -> T
    ) {
        viewModelScope.launch {
            stateFlow.value = UiState.Loading
            try {
                val result = block()
                stateFlow.value = UiState.Success(result)
            } catch (e: Exception) {
                Logger.e("${this@BaseViewModel::class.simpleName}: ${e.message}", e)
                stateFlow.value = UiState.Error(e.message ?: "Unknown error")
            }
        }
    }

    /**
     * Launch a fire-and-forget coroutine with error logging.
     * Use for operations where failure shouldn't affect UI state
     * (e.g., sending device tokens, analytics, non-critical updates).
     */
    protected fun launchSafely(
        onError: ((Exception) -> Unit)? = null,
        block: suspend () -> Unit
    ) {
        viewModelScope.launch {
            try {
                block()
            } catch (e: Exception) {
                Logger.e("${this@BaseViewModel::class.simpleName}: ${e.message}", e)
                onError?.invoke(e)
            }
        }
    }

    /**
     * Runs a capture/inspection form submission with the shared submitting → success/error
     * lifecycle. [setSubmitting] applies the three submission flags onto the form state via a
     * `copy(...)` on the concrete state type.
     */
    protected fun <S> submitForm(
        state: MutableStateFlow<S>,
        canSubmit: Boolean,
        setSubmitting: S.(isSubmitting: Boolean, error: String?, isSuccess: Boolean) -> S,
        perform: suspend () -> Unit
    ) {
        if (!canSubmit) return
        launchSafely(onError = { e ->
            state.update { it.setSubmitting(false, e.message ?: "Submission failed", false) }
        }) {
            state.update { it.setSubmitting(true, null, false) }
            perform()
            state.update { it.setSubmitting(false, null, true) }
        }
    }
}
