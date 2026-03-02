package com.logisticsx.driver.viewmodel.base

/**
 * Generic UI state for screens with simple Loading → Success/Error flows.
 * Use this for list/detail screens. For complex form screens with many
 * independent fields, prefer a dedicated data class instead.
 */
sealed class UiState<out T> {
    data object Loading : UiState<Nothing>()
    data class Success<T>(val data: T) : UiState<T>()
    data class Error(val message: String) : UiState<Nothing>()
}
