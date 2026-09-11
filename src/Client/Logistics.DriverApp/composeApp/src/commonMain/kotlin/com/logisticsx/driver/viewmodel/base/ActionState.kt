package com.logisticsx.driver.viewmodel.base

/**
 * State of a one-shot action such as save, create or delete.
 *
 * Use this rather than adding another per-ViewModel sealed class. [T] is the success result, or
 * [Unit] when the action returns nothing.
 */
sealed class ActionState<out T> {
    data object Idle : ActionState<Nothing>()
    data object Loading : ActionState<Nothing>()
    data class Success<T>(val data: T) : ActionState<T>()
    data class Error(val message: String) : ActionState<Nothing>()
}
