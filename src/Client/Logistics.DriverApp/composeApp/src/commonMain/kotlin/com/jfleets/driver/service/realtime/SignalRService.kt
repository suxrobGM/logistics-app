package com.jfleets.driver.service.realtime

import kotlinx.coroutines.flow.StateFlow

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
    suspend fun sendLocationUpdate(location: TruckGeolocation)

    /**
     * Checks if the connection is currently active.
     */
    fun isConnected(): Boolean
}
