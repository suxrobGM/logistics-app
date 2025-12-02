package com.jfleets.driver.service

import com.jfleets.driver.util.Logger
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

/**
 * iOS implementation of SignalRService.
 * TODO: Implement using Swift SignalR client via Kotlin/Native interop
 * or use a pure Kotlin WebSocket-based implementation.
 */
actual class SignalRService(
    private val hubUrl: String,
    private val preferencesManager: PreferencesManager
) {
    private val _connectionState = MutableStateFlow(SignalRConnectionState.DISCONNECTED)

    actual val connectionState: StateFlow<SignalRConnectionState> = _connectionState.asStateFlow()

    actual suspend fun connect() {
        Logger.w("SignalR not yet implemented for iOS")
        // TODO: Implement iOS SignalR client
        // Options:
        // 1. Use Swift SignalR client via cinterop
        // 2. Implement WebSocket-based SignalR protocol
        // 3. Use a KMP-compatible WebSocket library
    }

    actual suspend fun disconnect() {
        Logger.w("SignalR not yet implemented for iOS")
        _connectionState.value = SignalRConnectionState.DISCONNECTED
    }

    actual suspend fun sendLocationUpdate(location: LocationUpdate) {
        Logger.w("SignalR not yet implemented for iOS - location not sent: ${location.latitude}, ${location.longitude}")
    }

    actual fun isConnected(): Boolean {
        return _connectionState.value == SignalRConnectionState.CONNECTED
    }
}
