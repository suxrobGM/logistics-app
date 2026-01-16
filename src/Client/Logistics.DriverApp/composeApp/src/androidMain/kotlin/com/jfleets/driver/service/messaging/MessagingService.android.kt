package com.jfleets.driver.service.messaging

import com.jfleets.driver.api.models.MessageDto
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.util.Logger
import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import com.microsoft.signalr.HubConnectionState
import io.reactivex.rxjava3.core.Single
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.withContext
import kotlin.time.Instant

actual class MessagingService(
    private val hubUrl: String,
    private val preferencesManager: PreferencesManager
) {
    private var hubConnection: HubConnection? = null
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
        if (hubConnection?.connectionState == HubConnectionState.CONNECTED) {
            return
        }

        _connectionState.value = MessagingConnectionState.CONNECTING

        withContext(Dispatchers.IO) {
            try {
                val token = preferencesManager.getAccessToken()
                val tenantId = preferencesManager.getTenantId()
                val userId = preferencesManager.getUserId()

                hubConnection = HubConnectionBuilder.create(hubUrl)
                    .withAccessTokenProvider(Single.just(token ?: ""))
                    .withHeader("X-Tenant", tenantId ?: "")
                    .build()

                setupMessageHandlers()
                setupConnectionCallbacks()

                hubConnection?.start()?.blockingAwait()
                _connectionState.value = MessagingConnectionState.CONNECTED

                // Register tenant and user after connection
                tenantId?.let { registerTenant(it) }
                userId?.let { registerUser(it) }
            } catch (e: Exception) {
                _connectionState.value = MessagingConnectionState.DISCONNECTED
                Logger.e("MessagingService: Failed to connect", e)
                throw e
            }
        }
    }

    @Suppress("UNCHECKED_CAST")
    private fun setupMessageHandlers() {
        // Handle incoming messages - SignalR sends as Map via Gson
        hubConnection?.on("ReceiveMessage", { messageData: Any? ->
            try {
                val data = messageData as? Map<String, Any?> ?: return@on

                val message = MessageDto(
                    id = data["id"]?.toString(),
                    conversationId = data["conversationId"]?.toString(),
                    senderId = data["senderId"]?.toString(),
                    senderName = data["senderName"]?.toString(),
                    content = data["content"]?.toString(),
                    sentAt = (data["sentAt"] as? String)?.let {
                        try {
                            Instant.parse(it)
                        } catch (e: Exception) {
                            null
                        }
                    },
                    isRead = data["isRead"] as? Boolean,
                    isDeleted = data["isDeleted"] as? Boolean
                )
                kotlinx.coroutines.runBlocking {
                    _newMessages.emit(message)
                }
            } catch (e: Exception) {
                Logger.e("MessagingService: Error parsing message: ${e.message}", e)
            }
        }, Object::class.java)

        // Handle typing indicators - SignalR sends as LinkedHashMap
        hubConnection?.on("TypingIndicator", { indicatorData ->
            try {
                val data = indicatorData as? LinkedHashMap<String, Any?> ?: return@on
                kotlinx.coroutines.runBlocking {
                    _typingIndicators.emit(
                        TypingIndicator(
                            conversationId = data["conversationId"]?.toString() ?: "",
                            userId = data["userId"]?.toString() ?: "",
                            isTyping = data["isTyping"] as? Boolean ?: false
                        )
                    )
                }
            } catch (e: Exception) {
                Logger.e("MessagingService: Error parsing typing indicator", e)
            }
        }, Any::class.java)

        // Handle message read notifications
        hubConnection?.on("MessageRead", { messageId, readById ->
            kotlinx.coroutines.runBlocking {
                _messageReadNotifications.emit(
                    MessageReadNotification(
                        messageId = messageId,
                        readById = readById
                    )
                )
            }
        }, String::class.java, String::class.java)
    }

    private fun setupConnectionCallbacks() {
        hubConnection?.onClosed { exception ->
            _connectionState.value = MessagingConnectionState.DISCONNECTED
            if (exception != null) {
                Logger.e("MessagingService: Connection closed with error", exception)
            }
        }
    }

    private fun registerTenant(tenantId: String) {
        try {
            hubConnection?.send("RegisterTenant", tenantId)
        } catch (e: Exception) {
            Logger.e("MessagingService: Failed to register tenant", e)
        }
    }

    private fun registerUser(userId: String) {
        try {
            hubConnection?.send("RegisterUser", userId)
        } catch (e: Exception) {
            Logger.e("MessagingService: Failed to register user", e)
        }
    }

    actual suspend fun disconnect() {
        withContext(Dispatchers.IO) {
            try {
                hubConnection?.stop()?.blockingAwait()
                hubConnection = null
                _connectionState.value = MessagingConnectionState.DISCONNECTED
            } catch (e: Exception) {
                Logger.e("MessagingService: Error disconnecting", e)
            }
        }
    }

    actual fun joinConversation(conversationId: String) {
        if (hubConnection?.connectionState != HubConnectionState.CONNECTED) {
            Logger.w("MessagingService: Cannot join conversation - not connected")
            return
        }

        try {
            hubConnection?.send("JoinConversation", conversationId)
        } catch (e: Exception) {
            Logger.e("MessagingService: Failed to join conversation", e)
        }
    }

    actual fun leaveConversation(conversationId: String) {
        if (hubConnection?.connectionState != HubConnectionState.CONNECTED) {
            return
        }

        try {
            hubConnection?.send("LeaveConversation", conversationId)
        } catch (e: Exception) {
            Logger.e("MessagingService: Failed to leave conversation", e)
        }
    }

    actual fun sendTypingIndicator(conversationId: String, isTyping: Boolean) {
        if (hubConnection?.connectionState != HubConnectionState.CONNECTED) {
            return
        }

        try {
            hubConnection?.send("SendTypingIndicator", conversationId, isTyping)
        } catch (e: Exception) {
            Logger.e("MessagingService: Failed to send typing indicator", e)
        }
    }

    actual suspend fun markAsRead(conversationId: String, messageId: String) {
        if (hubConnection?.connectionState != HubConnectionState.CONNECTED) {
            return
        }

        try {
            val userId = preferencesManager.getUserId() ?: return
            hubConnection?.send("MarkAsRead", conversationId, messageId, userId)
        } catch (e: Exception) {
            Logger.e("MessagingService: Failed to mark message as read", e)
        }
    }

    actual fun isConnected(): Boolean {
        return hubConnection?.connectionState == HubConnectionState.CONNECTED
    }
}
