package com.jfleets.driver.service.messaging

import com.jfleets.driver.api.models.MessageDto
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.serialization.Serializable

/**
 * Typing indicator data received from SignalR.
 */
@Serializable
data class TypingIndicator(
    val conversationId: String,
    val userId: String,
    val isTyping: Boolean
)

/**
 * Message read notification data received from SignalR.
 */
data class MessageReadNotification(
    val messageId: String,
    val readById: String
)

/**
 * Connection state for the messaging SignalR hub.
 */
enum class MessagingConnectionState {
    DISCONNECTED,
    CONNECTING,
    CONNECTED,
    RECONNECTING
}

/**
 * Cross-platform messaging service for real-time chat communication.
 * Uses platform-specific SignalR implementations for real-time updates.
 */
expect class MessagingService {
    /**
     * Current connection state as a StateFlow for observability.
     */
    val connectionState: StateFlow<MessagingConnectionState>

    /**
     * Flow of new messages received from SignalR.
     */
    val newMessages: SharedFlow<MessageDto>

    /**
     * Flow of typing indicators received from SignalR.
     */
    val typingIndicators: SharedFlow<TypingIndicator>

    /**
     * Flow of message read notifications received from SignalR.
     */
    val messageReadNotifications: SharedFlow<MessageReadNotification>

    /**
     * Starts the SignalR connection to the messaging hub.
     */
    suspend fun connect()

    /**
     * Stops the SignalR connection.
     */
    suspend fun disconnect()

    /**
     * Joins a conversation to receive messages.
     */
    fun joinConversation(conversationId: String)

    /**
     * Leaves a conversation.
     */
    fun leaveConversation(conversationId: String)

    /**
     * Sends a typing indicator to a conversation.
     */
    fun sendTypingIndicator(conversationId: String, isTyping: Boolean)

    /**
     * Notifies that a message has been read.
     */
    suspend fun markAsRead(conversationId: String, messageId: String)

    /**
     * Checks if the connection is currently active.
     */
    fun isConnected(): Boolean
}
