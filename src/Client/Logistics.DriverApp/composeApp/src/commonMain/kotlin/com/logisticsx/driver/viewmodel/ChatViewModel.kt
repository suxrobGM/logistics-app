package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.MessageApi
import com.logisticsx.driver.api.bodyOrThrow
import com.logisticsx.driver.api.models.ConversationDto
import com.logisticsx.driver.api.models.MessageDto
import com.logisticsx.driver.api.models.SendMessageRequest
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.service.messaging.ConversationStateManager
import com.logisticsx.driver.service.messaging.MessagingService
import com.logisticsx.driver.util.Logger
import com.logisticsx.driver.viewmodel.base.BaseViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

data class ChatData(
    val messages: List<MessageDto>,
    val conversation: ConversationDto?,
    val hasMore: Boolean,
    val currentUserId: String?
)

class ChatViewModel(
    private val messageApi: MessageApi,
    private val preferencesManager: PreferencesManager,
    private val messagingService: MessagingService,
    private val conversationStateManager: ConversationStateManager,
    private val conversationId: String
) : BaseViewModel() {

    private val messageLoadBatchSize = 10
    private val _uiState = MutableStateFlow<UiState<ChatData>>(UiState.Loading)
    val uiState: StateFlow<UiState<ChatData>> = _uiState.asStateFlow()

    private var currentUserId: String? = null

    init {
        loadChat()
        observeRealTimeMessages()
    }

    private fun loadChat() {
        launchSafely {
            currentUserId = preferencesManager.getUserId()
            messagingService.joinConversation(conversationId)
            Logger.d("ChatViewModel: Joined conversation $conversationId")
            loadMessages()
        }
    }

    private fun observeRealTimeMessages() {
        // Observe new messages from other users
        launchSafely {
            conversationStateManager.newMessageReceived.collect { message ->
                if (message.conversationId != conversationId || message.senderId == currentUserId) {
                    return@collect
                }

                val currentState = _uiState.value
                if (currentState is UiState.Success) {
                    val messageExists = currentState.data.messages.any { it.id == message.id }
                    if (!messageExists) {
                        _uiState.value = UiState.Success(
                            currentState.data.copy(
                                messages = currentState.data.messages + message
                            )
                        )
                    }
                }
            }
        }

        // Observe message read notifications
        launchSafely {
            messagingService.messageReadNotifications.collect { notification ->
                val currentState = _uiState.value
                if (currentState is UiState.Success) {
                    _uiState.value = UiState.Success(
                        currentState.data.copy(
                            messages = currentState.data.messages.map {
                                if (it.id == notification.messageId) it.copy(isRead = true) else it
                            }
                        )
                    )
                }
            }
        }
    }

    fun loadMessages(append: Boolean = false) {
        launchSafely(onError = { e ->
            _uiState.value = UiState.Error(e.message ?: "Failed to load messages")
        }) {
            if (!append) {
                _uiState.value = UiState.Loading
            }

            val currentMessages = when (val state = _uiState.value) {
                is UiState.Success -> state.data.messages
                else -> emptyList()
            }

            val before =
                if (append && currentMessages.isNotEmpty()) currentMessages.first().sentAt else null

            val newMessages = messageApi.getMessages(
                conversationId = conversationId,
                limit = messageLoadBatchSize,
                before = before
            ).bodyOrThrow()

            val hasMore = newMessages.size >= messageLoadBatchSize
            val allMessages = if (append) newMessages + currentMessages else newMessages

            val conversations = try {
                messageApi.getConversations(participantId = currentUserId).bodyOrThrow()
            } catch (_: Exception) {
                emptyList()
            }
            val conversation = conversations.find { it.id == conversationId }

            _uiState.value = UiState.Success(
                ChatData(
                    messages = allMessages,
                    conversation = conversation,
                    hasMore = hasMore,
                    currentUserId = currentUserId
                )
            )
        }
    }

    fun sendMessage(content: String) {
        if (content.isBlank()) return

        launchSafely {
            val message = messageApi.sendMessage(
                SendMessageRequest(conversationId = conversationId, content = content.trim())
            ).bodyOrThrow()

            val currentState = _uiState.value
            if (currentState is UiState.Success) {
                _uiState.value = UiState.Success(
                    currentState.data.copy(
                        messages = currentState.data.messages + message
                    )
                )
            }
        }
    }

    fun markAsRead(messageId: String) {
        launchSafely {
            messageApi.markMessageRead(messageId).bodyOrThrow()

            val currentState = _uiState.value
            if (currentState is UiState.Success) {
                _uiState.value = UiState.Success(
                    currentState.data.copy(
                        messages = currentState.data.messages.map {
                            if (it.id == messageId) it.copy(isRead = true) else it
                        }
                    )
                )
            }

            conversationStateManager.decrementUnreadCount()
        }
    }

    fun isOwnMessage(message: MessageDto): Boolean {
        return message.senderId == currentUserId
    }

    override fun onCleared() {
        super.onCleared()
        launchSafely {
            messagingService.leaveConversation(conversationId)
            Logger.d("ChatViewModel: Left conversation $conversationId")
        }
    }
}
