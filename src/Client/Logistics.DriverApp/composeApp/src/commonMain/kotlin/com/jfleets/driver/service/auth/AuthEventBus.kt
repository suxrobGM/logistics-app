package com.jfleets.driver.service.auth

import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.asSharedFlow

/**
 * Sealed class representing authentication events that can occur during API calls.
 */
sealed class AuthEvent {
    /**
     * Emitted when a 401 Unauthorized response is received, indicating the user
     * should be redirected to the login screen.
     */
    data object Unauthorized : AuthEvent()
}

/**
 * Global event bus for authentication-related events.
 * Used to communicate auth failures from the HTTP layer to the UI layer.
 */
object AuthEventBus {
    private val _events = MutableSharedFlow<AuthEvent>(extraBufferCapacity = 1)
    val events = _events.asSharedFlow()

    /**
     * Emits an unauthorized event to trigger navigation to login.
     */
    fun emitUnauthorized() {
        _events.tryEmit(AuthEvent.Unauthorized)
    }
}
