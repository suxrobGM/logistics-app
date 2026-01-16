package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.MessageApi
import com.jfleets.driver.api.models.ConversationDto
import com.jfleets.driver.api.models.MessageDto
import com.jfleets.driver.api.models.SendMessageRequest
import com.jfleets.driver.service.PreferencesManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class MessagesViewModel(
    private val messageApi: MessageApi,
    private val preferencesManager: PreferencesManager
) : ViewModel() {

    private val _conversationsState =
        MutableStateFlow<ConversationsUiState>(ConversationsUiState.Loading)
    val conversationsState: StateFlow<ConversationsUiState> = _conversationsState.asStateFlow()

    private val _chatState = MutableStateFlow<ChatUiState>(ChatUiState.Initial)
    val chatState: StateFlow<ChatUiState> = _chatState.asStateFlow()

    private val _unreadCount = MutableStateFlow(0)
    val unreadCount: StateFlow<Int> = _unreadCount.asStateFlow()

    private var currentUserId: String? = null
    private var currentConversationId: String? = null

    init {
        loadConversations()
    }

    fun loadConversations() {
        viewModelScope.launch {
            _conversationsState.value = ConversationsUiState.Loading


            currentUserId = preferencesManager.getUserId()
            val response = messageApi.getConversations(participantId = currentUserId)

            if (!response.success) {
                _conversationsState.value = ConversationsUiState.Error(
                    "Failed to load conversations (${response.status})"
                )
                return@launch
            }

            val conversations = response.body()
            _conversationsState.value = ConversationsUiState.Success(conversations)

            // Update unread count
            val totalUnread = conversations.sumOf { it.unreadCount ?: 0 }
            _unreadCount.value = totalUnread

        }
    }

    fun selectConversation(conversationId: String) {
        currentConversationId = conversationId
        loadMessages()
    }

    fun loadMessages(append: Boolean = false) {
        val conversationId = currentConversationId ?: return

        viewModelScope.launch {
            if (!append) {
                _chatState.value = ChatUiState.Loading
            }


            val currentMessages = when (val state = _chatState.value) {
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
                _chatState.value =
                    ChatUiState.Error("Failed to load messages (${response.status})")
                return@launch
            }

            val newMessages = response.body()
            val hasMore = newMessages.size >= 50
            val allMessages = if (append) newMessages + currentMessages else newMessages

            // Find current conversation
            val conversation = when (val convState = _conversationsState.value) {
                is ConversationsUiState.Success -> convState.conversations.find { it.id == conversationId }
                else -> null
            }

            _chatState.value = ChatUiState.Success(
                messages = allMessages,
                conversation = conversation,
                hasMore = hasMore,
                currentUserId = currentUserId
            )
        }
    }

    fun sendMessage(content: String) {
        val conversationId = currentConversationId ?: return
        if (content.isBlank()) return

        viewModelScope.launch {

            val response = messageApi.sendMessage(
                SendMessageRequest(
                    conversationId = conversationId,
                    content = content.trim()
                )
            )

            if (!response.success) {
                // Could show a toast or error state
                return@launch
            }

            val message = response.body()

            // Add message to current list
            val currentState = _chatState.value
            if (currentState is ChatUiState.Success) {
                _chatState.value = currentState.copy(
                    messages = currentState.messages + message
                )
            }

            // Update conversation list with new last message
            updateConversationLastMessage(conversationId, message)
        }
    }

    fun markAsRead(messageId: String) {
        viewModelScope.launch {
            try {
                messageApi.markMessageRead(messageId)

                // Update local state
                val currentState = _chatState.value
                if (currentState is ChatUiState.Success) {
                    _chatState.value = currentState.copy(
                        messages = currentState.messages.map {
                            if (it.id == messageId) it.copy(isRead = true) else it
                        }
                    )
                }

                // Decrement unread count
                if (_unreadCount.value > 0) {
                    _unreadCount.value--
                }
            } catch (e: Exception) {
                // Silently fail
            }
        }
    }

    private fun updateConversationLastMessage(conversationId: String, message: MessageDto) {
        val currentState = _conversationsState.value
        if (currentState is ConversationsUiState.Success) {
            _conversationsState.value = currentState.copy(
                conversations = currentState.conversations.map {
                    if (it.id == conversationId) {
                        it.copy(
                            lastMessage = message,
                            lastMessageAt = message.sentAt
                        )
                    } else it
                }
            )
        }
    }

    fun clearChat() {
        currentConversationId = null
        _chatState.value = ChatUiState.Initial
    }

    fun refresh() {
        loadConversations()
    }

    fun isOwnMessage(message: MessageDto): Boolean {
        return message.senderId == currentUserId
    }
}

sealed class ConversationsUiState {
    object Loading : ConversationsUiState()
    data class Success(val conversations: List<ConversationDto>) : ConversationsUiState()
    data class Error(val message: String) : ConversationsUiState()
}

sealed class ChatUiState {
    object Initial : ChatUiState()
    object Loading : ChatUiState()
    data class Success(
        val messages: List<MessageDto>,
        val conversation: ConversationDto?,
        val hasMore: Boolean,
        val currentUserId: String?
    ) : ChatUiState()

    data class Error(val message: String) : ChatUiState()
}
