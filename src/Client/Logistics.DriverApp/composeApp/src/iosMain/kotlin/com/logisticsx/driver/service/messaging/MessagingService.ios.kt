@file:OptIn(kotlin.time.ExperimentalTime::class)

package com.logisticsx.driver.service.messaging

import com.logisticsx.driver.api.models.MessageDto
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.service.realtime.SignalRWebSocketClient
import com.logisticsx.driver.util.Logger
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.boolean
import kotlinx.serialization.json.booleanOrNull
import kotlinx.serialization.json.jsonArray
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import kotlin.time.Instant

/**
 * iOS implementation of MessagingService using the shared SignalRWebSocketClient
 * (Ktor WebSocket + SignalR JSON Hub Protocol).
 */
actual class MessagingService(
    private val hubUrl: String,
    private val preferencesManager: PreferencesManager
) {
    private var client: SignalRWebSocketClient? = null
    private val scope = CoroutineScope(Dispatchers.Default + SupervisorJob())

    private val _connectionState = MutableStateFlow(MessagingConnectionState.DISCONNECTED)
    private val _newMessages = MutableSharedFlow<MessageDto>()
    private val _typingIndicators = MutableSharedFlow<TypingIndicator>()
    private val _messageReadNotifications = MutableSharedFlow<MessageReadNotification>()

    actual val connectionState: StateFlow<MessagingConnectionState> = _connectionState.asStateFlow()
    actual val newMessages: SharedFlow<MessageDto> = _newMessages.asSharedFlow()
    actual val typingIndicators: SharedFlow<TypingIndicator> = _typingIndicators.asSharedFlow()
    actual val messageReadNotifications: SharedFlow<MessageReadNotification> =
        _messageReadNotifications.asSharedFlow()

    actual suspend fun connect() {
        if (_connectionState.value == MessagingConnectionState.CONNECTED) {
            return
        }

        _connectionState.value = MessagingConnectionState.CONNECTING

        try {
            val token = preferencesManager.getAccessToken() ?: ""
            val tenantId = preferencesManager.getTenantId() ?: ""
            val userId = preferencesManager.getUserId() ?: ""

            val wsClient = SignalRWebSocketClient(
                hubUrl = hubUrl,
                accessToken = token,
                tenantId = tenantId
            )

            setupMessageHandlers(wsClient)

            client = wsClient
            wsClient.connect()

            // Register tenant and user before marking as connected
            if (tenantId.isNotEmpty()) {
                wsClient.send("RegisterTenant", JsonPrimitive(tenantId))
            }
            if (userId.isNotEmpty()) {
                wsClient.send("RegisterUser", JsonPrimitive(userId))
            }

            _connectionState.value = MessagingConnectionState.CONNECTED
            Logger.d("MessagingService iOS: Connected successfully")
        } catch (e: Exception) {
            _connectionState.value = MessagingConnectionState.DISCONNECTED
            Logger.e("MessagingService iOS: Connection failed: ${e.message}")
            throw e
        }
    }

    private fun setupMessageHandlers(wsClient: SignalRWebSocketClient) {
        wsClient.on("ReceiveMessage") { msg ->
            try {
                val args = msg["arguments"]?.jsonArray ?: return@on
                if (args.isEmpty()) return@on
                val data = args[0].jsonObject

                val message = MessageDto(
                    id = data["id"]?.jsonPrimitive?.content,
                    conversationId = data["conversationId"]?.jsonPrimitive?.content,
                    senderId = data["senderId"]?.jsonPrimitive?.content,
                    senderName = data["senderName"]?.jsonPrimitive?.content,
                    content = data["content"]?.jsonPrimitive?.content,
                    sentAt = data["sentAt"]?.jsonPrimitive?.content?.let { str ->
                        try {
                            Instant.parse(str)
                        } catch (_: Exception) {
                            null
                        }
                    },
                    isRead = data["isRead"]?.jsonPrimitive?.booleanOrNull,
                    isDeleted = data["isDeleted"]?.jsonPrimitive?.booleanOrNull
                )
                scope.launch { _newMessages.emit(message) }
            } catch (e: Exception) {
                Logger.e("MessagingService iOS: Error parsing message: ${e.message}")
            }
        }

        wsClient.on("TypingIndicator") { msg ->
            try {
                val args = msg["arguments"]?.jsonArray ?: return@on
                if (args.isEmpty()) return@on
                val data = args[0].jsonObject

                val indicator = TypingIndicator(
                    conversationId = data["conversationId"]?.jsonPrimitive?.content ?: "",
                    userId = data["userId"]?.jsonPrimitive?.content ?: "",
                    isTyping = data["isTyping"]?.jsonPrimitive?.boolean ?: false
                )
                scope.launch { _typingIndicators.emit(indicator) }
            } catch (e: Exception) {
                Logger.e("MessagingService iOS: Error parsing typing indicator: ${e.message}")
            }
        }

        wsClient.on("MessageRead") { msg ->
            try {
                val args = msg["arguments"]?.jsonArray ?: return@on
                if (args.size < 2) return@on

                val messageId = args[0].jsonPrimitive.content
                val readById = args[1].jsonPrimitive.content

                scope.launch {
                    _messageReadNotifications.emit(
                        MessageReadNotification(messageId = messageId, readById = readById)
                    )
                }
            } catch (e: Exception) {
                Logger.e("MessagingService iOS: Error parsing message read: ${e.message}")
            }
        }
    }

    actual suspend fun disconnect() {
        try {
            client?.disconnect()
            client = null
            _connectionState.value = MessagingConnectionState.DISCONNECTED
            Logger.d("MessagingService iOS: Disconnected")
        } catch (e: Exception) {
            Logger.e("MessagingService iOS: Disconnect error: ${e.message}")
        }
    }

    actual fun joinConversation(conversationId: String) {
        if (_connectionState.value != MessagingConnectionState.CONNECTED) {
            Logger.w("MessagingService iOS: Cannot join conversation - not connected")
            return
        }

        try {
            scope.launch {
                client?.send("JoinConversation", JsonPrimitive(conversationId))
            }
        } catch (e: Exception) {
            Logger.e("MessagingService iOS: Failed to join conversation: ${e.message}")
        }
    }

    actual fun leaveConversation(conversationId: String) {
        if (_connectionState.value != MessagingConnectionState.CONNECTED) {
            return
        }

        try {
            scope.launch {
                client?.send("LeaveConversation", JsonPrimitive(conversationId))
            }
        } catch (e: Exception) {
            Logger.e("MessagingService iOS: Failed to leave conversation: ${e.message}")
        }
    }

    actual fun sendTypingIndicator(conversationId: String, isTyping: Boolean) {
        if (_connectionState.value != MessagingConnectionState.CONNECTED) {
            return
        }

        try {
            scope.launch {
                client?.send(
                    "SendTypingIndicator",
                    JsonPrimitive(conversationId),
                    JsonPrimitive(isTyping)
                )
            }
        } catch (e: Exception) {
            Logger.e("MessagingService iOS: Failed to send typing indicator: ${e.message}")
        }
    }

    actual suspend fun markAsRead(conversationId: String, messageId: String) {
        if (_connectionState.value != MessagingConnectionState.CONNECTED) {
            return
        }

        try {
            val userId = preferencesManager.getUserId() ?: return
            client?.send(
                "MarkAsRead",
                JsonPrimitive(conversationId),
                JsonPrimitive(messageId),
                JsonPrimitive(userId)
            )
        } catch (e: Exception) {
            Logger.e("MessagingService iOS: Failed to mark message as read: ${e.message}")
        }
    }

    actual fun isConnected(): Boolean {
        return client?.isConnected == true
    }
}
