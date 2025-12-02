package com.jfleets.driver.service.realtime

import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.util.Logger
import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import com.microsoft.signalr.HubConnectionState
import io.reactivex.rxjava3.core.Single
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.withContext

actual class SignalRService(
    private val hubUrl: String,
    private val preferencesManager: PreferencesManager
) {
    private var hubConnection: HubConnection? = null
    private val _connectionState = MutableStateFlow(SignalRConnectionState.DISCONNECTED)

    actual val connectionState: StateFlow<SignalRConnectionState> = _connectionState.asStateFlow()

    actual suspend fun connect() {
        if (hubConnection?.connectionState == HubConnectionState.CONNECTED) {
            Logger.d("SignalR already connected")
            return
        }

        _connectionState.value = SignalRConnectionState.CONNECTING

        withContext(Dispatchers.IO) {
            try {
                val token = preferencesManager.getAccessToken()
                val tenantId = preferencesManager.getTenantId()

                hubConnection = HubConnectionBuilder.create(hubUrl)
                    .withAccessTokenProvider(Single.just(token ?: ""))
                    .withHeader("X-Tenant", tenantId ?: "")
                    .build()

                setupConnectionCallbacks()

                hubConnection?.start()?.blockingAwait()
                _connectionState.value = SignalRConnectionState.CONNECTED
                Logger.d("SignalR connected successfully")
            } catch (e: Exception) {
                _connectionState.value = SignalRConnectionState.DISCONNECTED
                Logger.e("Failed to connect SignalR", e)
                throw e
            }
        }
    }

    private fun setupConnectionCallbacks() {
        hubConnection?.onClosed { exception ->
            _connectionState.value = SignalRConnectionState.DISCONNECTED
            if (exception != null) {
                Logger.e("SignalR connection closed with error", exception)
            } else {
                Logger.d("SignalR connection closed")
            }
        }
    }

    actual suspend fun disconnect() {
        withContext(Dispatchers.IO) {
            try {
                hubConnection?.stop()?.blockingAwait()
                hubConnection = null
                _connectionState.value = SignalRConnectionState.DISCONNECTED
                Logger.d("SignalR disconnected")
            } catch (e: Exception) {
                Logger.e("Error disconnecting SignalR", e)
            }
        }
    }

    actual suspend fun sendLocationUpdate(location: TruckGeolocation) {
        if (hubConnection?.connectionState != HubConnectionState.CONNECTED) {
            Logger.w("Cannot send location update: SignalR not connected")
            return
        }

        withContext(Dispatchers.IO) {
            try {
                hubConnection?.send(
                    "SendGeolocationData",
                    location
                )
                Logger.d("Location sent: ${location.currentLocation.latitude}, ${location.currentLocation.longitude}")
            } catch (e: Exception) {
                Logger.e("Failed to send location update", e)
            }
        }
    }

    actual fun isConnected(): Boolean {
        return hubConnection?.connectionState == HubConnectionState.CONNECTED
    }
}
