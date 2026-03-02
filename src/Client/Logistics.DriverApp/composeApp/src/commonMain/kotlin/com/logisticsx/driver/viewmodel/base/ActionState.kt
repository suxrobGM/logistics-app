package com.logisticsx.driver.viewmodel.base

/**
 * Generic state for one-shot actions (save, create, delete).
 * Use this instead of defining per-ViewModel sealed classes like
 * CreateConversationState, TeamChatState, SaveState, etc.
 *
 * @param T the result type on success (use [Unit] for actions with no result,
 *          or [String] for actions returning an ID, etc.)
 */
sealed class ActionState<out T> {
    data object Idle : ActionState<Nothing>()
    data object Loading : ActionState<Nothing>()
    data class Success<T>(val data: T) : ActionState<T>()
    data class Error(val message: String) : ActionState<Nothing>()
}
