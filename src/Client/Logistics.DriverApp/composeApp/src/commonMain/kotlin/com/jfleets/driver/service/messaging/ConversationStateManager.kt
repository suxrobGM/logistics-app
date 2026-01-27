package com.jfleets.driver.service.messaging

import com.jfleets.driver.api.models.MessageDto
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.util.Logger
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

/**
 * Manages shared messaging state across screens.
 * Handles real-time message observation and unread count tracking.
 */
class ConversationStateManager(
    private val messagingService: MessagingService,
    private val preferencesManager: PreferencesManager
) {
    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.Main)

    private val _unreadCount = MutableStateFlow(0)
    val unreadCount: StateFlow<Int> = _unreadCount.asStateFlow()

    private val _newMessageReceived = MutableSharedFlow<MessageDto>(extraBufferCapacity = 10)
    val newMessageReceived: SharedFlow<MessageDto> = _newMessageReceived.asSharedFlow()

    private val _conversationUpdated = MutableSharedFlow<String>(extraBufferCapacity = 10)
    val conversationUpdated: SharedFlow<String> = _conversationUpdated.asSharedFlow()

    init {
        connectAndObserve()
    }

    private fun connectAndObserve() {
        scope.launch {
            try {
                messagingService.connect()
                Logger.d("ConversationStateManager: Connected to chat hub")
            } catch (e: Exception) {
                Logger.e("ConversationStateManager: Failed to connect - ${e.message}")
            }
        }

        // Observe new messages
        scope.launch {
            messagingService.newMessages.collect { message ->
                Logger.d("ConversationStateManager: Received message from ${message.senderName}")
                _newMessageReceived.emit(message)
                message.conversationId?.let { _conversationUpdated.emit(it) }
            }
        }
    }

    fun updateUnreadCount(count: Int) {
        _unreadCount.value = count
    }

    fun decrementUnreadCount() {
        if (_unreadCount.value > 0) {
            _unreadCount.value--
        }
    }

    fun incrementUnreadCount() {
        _unreadCount.value++
    }

    suspend fun getCurrentUserId(): String? {
        return preferencesManager.getUserId()
    }
}
