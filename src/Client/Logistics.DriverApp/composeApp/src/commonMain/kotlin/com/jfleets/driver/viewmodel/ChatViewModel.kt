package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.MessageApi
import com.jfleets.driver.api.models.ConversationDto
import com.jfleets.driver.api.models.MessageDto
import com.jfleets.driver.api.models.SendMessageRequest
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.messaging.ConversationStateManager
import com.jfleets.driver.service.messaging.MessagingService
import com.jfleets.driver.util.Logger
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class ChatViewModel(
    private val messageApi: MessageApi,
    private val preferencesManager: PreferencesManager,
    private val messagingService: MessagingService,
    private val conversationStateManager: ConversationStateManager,
    private val conversationId: String
) : ViewModel() {

    private val _uiState = MutableStateFlow<ChatUiState>(ChatUiState.Loading)
    val uiState: StateFlow<ChatUiState> = _uiState.asStateFlow()

    private var currentUserId: String? = null

    init {
        loadChat()
        observeRealTimeMessages()
    }

    private fun loadChat() {
        viewModelScope.launch {
            currentUserId = preferencesManager.getUserId()

            // Join conversation for real-time updates
            messagingService.joinConversation(conversationId)
            Logger.d("ChatViewModel: Joined conversation $conversationId")

            loadMessages()
        }
    }

    private fun observeRealTimeMessages() {
        // Observe new messages from other users (skip own messages - they're added locally when sent)
        viewModelScope.launch {
            conversationStateManager.newMessageReceived.collect { message ->
                if (message.conversationId != conversationId || message.senderId == currentUserId) {
                    return@collect
                }

                val currentState = _uiState.value
                if (currentState is ChatUiState.Success) {
                    val messageExists = currentState.messages.any { it.id == message.id }
                    if (!messageExists) {
                        _uiState.value = currentState.copy(
                            messages = currentState.messages + message
                        )
                    }
                }
            }
        }

        // Observe message read notifications
        viewModelScope.launch {
            messagingService.messageReadNotifications.collect { notification ->
                val currentState = _uiState.value
                if (currentState is ChatUiState.Success) {
                    _uiState.value = currentState.copy(
                        messages = currentState.messages.map {
                            if (it.id == notification.messageId) it.copy(isRead = true) else it
                        }
                    )
                }
            }
        }
    }

    fun loadMessages(append: Boolean = false) {
        viewModelScope.launch {
            if (!append) {
                _uiState.value = ChatUiState.Loading
            }

            val currentMessages = when (val state = _uiState.value) {
                is ChatUiState.Success -> state.messages
                else -> emptyList()
            }

            val before =
                if (append && currentMessages.isNotEmpty()) currentMessages.first().sentAt else null

            val response = messageApi.getMessages(
                conversationId = conversationId,
                limit = 50,
                before = before
            )

            if (!response.success) {
                _uiState.value = ChatUiState.Error("Failed to load messages (${response.status})")
                return@launch
            }

            val newMessages = response.body()
            val hasMore = newMessages.size >= 50
            val allMessages = if (append) newMessages + currentMessages else newMessages

            // Get conversation details from conversations list
            val conversationsResponse = messageApi.getConversations(participantId = currentUserId)
            val conversation = if (conversationsResponse.success) {
                conversationsResponse.body().find { it.id == conversationId }
            } else null

            _uiState.value = ChatUiState.Success(
                messages = allMessages,
                conversation = conversation,
                hasMore = hasMore,
                currentUserId = currentUserId
            )
        }
    }

    fun sendMessage(content: String) {
        if (content.isBlank()) return

        viewModelScope.launch {
            val response = messageApi.sendMessage(
                SendMessageRequest(
                    conversationId = conversationId,
                    content = content.trim()
                )
            )

            if (!response.success) {
                Logger.e("ChatViewModel: Failed to send message")
                return@launch
            }

            val message = response.body()

            val currentState = _uiState.value
            if (currentState is ChatUiState.Success) {
                _uiState.value = currentState.copy(
                    messages = currentState.messages + message
                )
            }
        }
    }

    fun markAsRead(messageId: String) {
        viewModelScope.launch {
            messageApi.markMessageRead(messageId)

            val currentState = _uiState.value
            if (currentState is ChatUiState.Success) {
                _uiState.value = currentState.copy(
                    messages = currentState.messages.map {
                        if (it.id == messageId) it.copy(isRead = true) else it
                    }
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
        viewModelScope.launch {
            messagingService.leaveConversation(conversationId)
            Logger.d("ChatViewModel: Left conversation $conversationId")
        }
    }
}

sealed class ChatUiState {
    data object Initial : ChatUiState()
    data object Loading : ChatUiState()
    data class Success(
        val messages: List<MessageDto>,
        val conversation: ConversationDto?,
        val hasMore: Boolean,
        val currentUserId: String?
    ) : ChatUiState()

    data class Error(val message: String) : ChatUiState()
}
