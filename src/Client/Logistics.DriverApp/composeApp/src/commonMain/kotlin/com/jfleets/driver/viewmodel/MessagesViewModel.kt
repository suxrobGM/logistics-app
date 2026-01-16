package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.MessageApi
import com.jfleets.driver.api.TruckApi
import com.jfleets.driver.api.models.ConversationDto
import com.jfleets.driver.api.models.CreateConversationRequest
import com.jfleets.driver.api.models.MessageDto
import com.jfleets.driver.api.models.SendMessageRequest
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.messaging.MessagingService
import com.jfleets.driver.util.Logger
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class DispatcherInfo(
    val id: String,
    val name: String
)

class MessagesViewModel(
    private val messageApi: MessageApi,
    private val driverApi: DriverApi,
    private val truckApi: TruckApi,
    private val preferencesManager: PreferencesManager,
    private val messagingService: MessagingService,
) : ViewModel() {

    private val _conversationsState =
        MutableStateFlow<ConversationsUiState>(ConversationsUiState.Loading)
    val conversationsState: StateFlow<ConversationsUiState> = _conversationsState.asStateFlow()

    private val _chatState = MutableStateFlow<ChatUiState>(ChatUiState.Initial)
    val chatState: StateFlow<ChatUiState> = _chatState.asStateFlow()

    private val _unreadCount = MutableStateFlow(0)
    val unreadCount: StateFlow<Int> = _unreadCount.asStateFlow()

    private val _dispatcherInfo = MutableStateFlow<DispatcherInfo?>(null)
    val dispatcherInfo: StateFlow<DispatcherInfo?> = _dispatcherInfo.asStateFlow()

    private val _createConversationState =
        MutableStateFlow<CreateConversationState>(CreateConversationState.Idle)
    val createConversationState: StateFlow<CreateConversationState> =
        _createConversationState.asStateFlow()

    private var currentUserId: String? = null
    private var currentConversationId: String? = null

    init {
        loadConversations()
        loadDispatcherInfo()
        connectToMessagingHub()
        observeNewMessages()
    }

    private fun connectToMessagingHub() {
        viewModelScope.launch {
            try {
                messagingService.connect()
                Logger.d("MessagesViewModel: Connected to messaging hub")
            } catch (e: Exception) {
                Logger.e("MessagesViewModel: Failed to connect to messaging hub - ${e.message}")
            }
        }
    }

    private fun observeNewMessages() {
        viewModelScope.launch {
            messagingService.newMessages.collect { message ->
                Logger.d("MessagesViewModel: Received new message from ${message.senderName}")

                // Only add message if it's for the current conversation
                if (message.conversationId == currentConversationId) {
                    val currentState = _chatState.value
                    if (currentState is ChatUiState.Success) {
                        // Avoid duplicates by checking if message already exists
                        val messageExists = currentState.messages.any { it.id == message.id }
                        if (!messageExists) {
                            _chatState.value = currentState.copy(
                                messages = currentState.messages + message
                            )
                        }
                    }
                }

                // Update conversation list with new last message
                message.conversationId?.let { updateConversationLastMessage(it, message) }
            }
        }

        // Observe message read notifications
        viewModelScope.launch {
            messagingService.messageReadNotifications.collect { notification ->
                val currentState = _chatState.value
                if (currentState is ChatUiState.Success) {
                    _chatState.value = currentState.copy(
                        messages = currentState.messages.map {
                            if (it.id == notification.messageId) it.copy(isRead = true) else it
                        }
                    )
                }
            }
        }
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
        // Leave previous conversation if any
        currentConversationId?.let { previousId ->
            viewModelScope.launch {
                messagingService.leaveConversation(previousId)
            }
        }

        currentConversationId = conversationId
        loadMessages()

        // Join new conversation for real-time updates
        viewModelScope.launch {
            messagingService.joinConversation(conversationId)
            Logger.d("MessagesViewModel: Joined conversation $conversationId")
        }
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
        // Leave the current conversation
        currentConversationId?.let { conversationId ->
            viewModelScope.launch {
                messagingService.leaveConversation(conversationId)
                Logger.d("MessagesViewModel: Left conversation $conversationId")
            }
        }
        currentConversationId = null
        _chatState.value = ChatUiState.Initial
    }

    fun refresh() {
        loadConversations()
    }

    fun isOwnMessage(message: MessageDto): Boolean {
        return message.senderId == currentUserId
    }

    private fun loadDispatcherInfo() {
        viewModelScope.launch {
            val userId = preferencesManager.getUserId() ?: return@launch

            // Get driver ID from user ID
            val driverResponse = driverApi.getDriverByUserId(userId)
            if (!driverResponse.success) return@launch

            val driver = driverResponse.body()
            val driverId = driver.id ?: return@launch

            // Get truck with active loads to find dispatcher
            val truckResponse = truckApi.getTruckById(
                driverId,
                includeLoads = true,
                onlyActiveLoads = true
            )
            if (!truckResponse.success) return@launch

            val truck = truckResponse.body()

            // Find first load with an assigned dispatcher
            val loadWithDispatcher = truck.loads?.firstOrNull {
                !it.assignedDispatcherId.isNullOrEmpty()
            }

            loadWithDispatcher?.let { load ->
                val dispatcherId = load.assignedDispatcherId
                val dispatcherName = load.assignedDispatcherName ?: "Dispatcher"
                if (!dispatcherId.isNullOrEmpty()) {
                    _dispatcherInfo.value = DispatcherInfo(
                        id = dispatcherId,
                        name = dispatcherName
                    )
                }
            }

        }
    }

    fun startConversationWithDispatcher() {
        val dispatcher = _dispatcherInfo.value ?: return
        val userId = currentUserId ?: return

        // Check if conversation with dispatcher already exists
        val existingConversation = (_conversationsState.value as? ConversationsUiState.Success)
            ?.conversations
            ?.find { conversation ->
                conversation.participants?.any { it.employeeId == dispatcher.id } == true
            }

        if (existingConversation != null) {
            // Navigate to existing conversation
            _createConversationState.value =
                CreateConversationState.Success(existingConversation.id!!)
            return
        }

        // Create new conversation
        viewModelScope.launch {
            _createConversationState.value = CreateConversationState.Creating


            val response = messageApi.createConversation(
                CreateConversationRequest(
                    participantIds = listOf(userId, dispatcher.id),
                    name = "Chat with ${dispatcher.name}"
                )
            )

            if (!response.success) {
                _createConversationState.value = CreateConversationState.Error(
                    "Failed to create conversation (${response.status})"
                )
                return@launch
            }

            val conversation = response.body()
            conversation.id?.let { conversationId ->
                // Refresh conversations list
                loadConversations()
                _createConversationState.value = CreateConversationState.Success(conversationId)
            }

        }
    }

    fun resetCreateConversationState() {
        _createConversationState.value = CreateConversationState.Idle
    }

    private val _tenantChatState = MutableStateFlow<TenantChatState>(TenantChatState.Idle)
    val tenantChatState: StateFlow<TenantChatState> = _tenantChatState.asStateFlow()

    fun openTeamChat() {
        viewModelScope.launch {
            _tenantChatState.value = TenantChatState.Loading

            val response = messageApi.getTenantChat()

            if (!response.success) {
                _tenantChatState.value = TenantChatState.Error(
                    "Failed to load team chat (${response.status})"
                )
                return@launch
            }

            val tenantChat = response.body()
            tenantChat.id?.let { conversationId ->
                // Refresh conversations to include tenant chat
                loadConversations()
                _tenantChatState.value = TenantChatState.Success(conversationId)
            }

        }
    }

    fun resetTenantChatState() {
        _tenantChatState.value = TenantChatState.Idle
    }
}

sealed class TenantChatState {
    object Idle : TenantChatState()
    object Loading : TenantChatState()
    data class Success(val conversationId: String) : TenantChatState()
    data class Error(val message: String) : TenantChatState()
}

sealed class CreateConversationState {
    object Idle : CreateConversationState()
    object Creating : CreateConversationState()
    data class Success(val conversationId: String) : CreateConversationState()
    data class Error(val message: String) : CreateConversationState()
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
