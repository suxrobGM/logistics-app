@file:OptIn(kotlinx.serialization.ExperimentalSerializationApi::class)

package com.logisticsx.driver.service.realtime

import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.util.Logger
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.serialization.json.JsonElement
import kotlinx.serialization.json.JsonNull
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive

/**
 * iOS implementation of SignalRService using the shared SignalRWebSocketClient
 * (Ktor WebSocket + SignalR JSON Hub Protocol).
 */
actual class SignalRService(
    private val hubUrl: String,
    private val preferencesManager: PreferencesManager
) {
    private var client: SignalRWebSocketClient? = null
    private val _connectionState = MutableStateFlow(SignalRConnectionState.DISCONNECTED)

    actual val connectionState: StateFlow<SignalRConnectionState> = _connectionState.asStateFlow()

    actual suspend fun connect() {
        if (_connectionState.value == SignalRConnectionState.CONNECTED) {
            Logger.d("SignalR iOS: Already connected")
            return
        }

        _connectionState.value = SignalRConnectionState.CONNECTING

        try {
            val token = preferencesManager.getAccessToken() ?: ""
            val tenantId = preferencesManager.getTenantId() ?: ""

            val wsClient = SignalRWebSocketClient(
                hubUrl = hubUrl,
                accessToken = token,
                tenantId = tenantId
            )
            client = wsClient

            wsClient.connect()
            _connectionState.value = SignalRConnectionState.CONNECTED
            Logger.d("SignalR iOS: Connected successfully")
        } catch (e: Exception) {
            _connectionState.value = SignalRConnectionState.DISCONNECTED
            Logger.e("SignalR iOS: Connection failed: ${e.message}")
            throw e
        }
    }

    actual suspend fun disconnect() {
        try {
            client?.disconnect()
            client = null
            _connectionState.value = SignalRConnectionState.DISCONNECTED
            Logger.d("SignalR iOS: Disconnected")
        } catch (e: Exception) {
            Logger.e("SignalR iOS: Disconnect error: ${e.message}")
        }
    }

    actual suspend fun sendLocationUpdate(location: TruckGeolocation) {
        if (_connectionState.value != SignalRConnectionState.CONNECTED) {
            Logger.w("SignalR iOS: Cannot send location - not connected")
            return
        }

        try {
            val currentLocation = JsonObject(mapOf(
                "latitude" to JsonPrimitive(location.currentLocation.latitude),
                "longitude" to JsonPrimitive(location.currentLocation.longitude)
            ))

            val address: JsonElement = if (location.currentAddress != null) {
                val addr = location.currentAddress
                JsonObject(mapOf(
                    "line1" to stringOrNull(addr.line1),
                    "line2" to stringOrNull(addr.line2),
                    "city" to stringOrNull(addr.city),
                    "state" to stringOrNull(addr.state),
                    "zipCode" to stringOrNull(addr.zipCode),
                    "country" to stringOrNull(addr.country)
                ))
            } else {
                JsonPrimitive(null as String?)
            }

            val locationJson = JsonObject(mapOf(
                "truckId" to JsonPrimitive(location.truckId),
                "tenantId" to JsonPrimitive(location.tenantId),
                "currentLocation" to currentLocation,
                "currentAddress" to address,
                "truckNumber" to stringOrNull(location.truckNumber),
                "driversName" to stringOrNull(location.driversName)
            ))

            client?.send("SendGeolocationData", locationJson)
            Logger.d("SignalR iOS: Location sent: ${location.currentLocation.latitude}, ${location.currentLocation.longitude}")
        } catch (e: Exception) {
            Logger.e("SignalR iOS: Failed to send location: ${e.message}")
        }
    }

    actual fun isConnected(): Boolean {
        return client?.isConnected == true
    }

    private fun stringOrNull(value: String?): JsonElement =
        if (value != null) JsonPrimitive(value) else JsonPrimitive(null as String?)
}
