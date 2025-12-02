package com.jfleets.driver.service

import kotlinx.coroutines.flow.StateFlow
import kotlin.time.Clock

/**
 * Connection state for SignalR hub.
 */
enum class SignalRConnectionState {
    DISCONNECTED,
    CONNECTING,
    CONNECTED,
    RECONNECTING
}

/**
 * Location data to send to the server.
 */
data class LocationUpdate(
    val latitude: Double,
    val longitude: Double,
    val address: String? = null,
    val timestamp: Long = Clock.System.now().toEpochMilliseconds()
)

/**
 * Cross-platform SignalR service for real-time communication with the backend.
 * Uses platform-specific implementations:
 * - Android: Microsoft SignalR Java Client
 * - iOS: Swift SignalR Client
 */
expect class SignalRService {
    /**
     * Current connection state as a StateFlow for observability.
     */
    val connectionState: StateFlow<SignalRConnectionState>

    /**
     * Starts the SignalR connection to the hub.
     */
    suspend fun connect()

    /**
     * Stops the SignalR connection.
     */
    suspend fun disconnect()

    /**
     * Sends a location update to the server.
     * @param location The location data to send.
     */
    suspend fun sendLocationUpdate(location: LocationUpdate)

    /**
     * Checks if the connection is currently active.
     */
    fun isConnected(): Boolean
}
