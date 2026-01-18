package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.DriverApi
import com.jfleets.driver.api.MessageApi
import com.jfleets.driver.api.TruckApi
import com.jfleets.driver.api.models.ConversationDto
import com.jfleets.driver.api.models.CreateConversationRequest
import com.jfleets.driver.service.PreferencesManager
import com.jfleets.driver.service.messaging.ConversationStateManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class DispatcherInfo(
    val id: String,
    val name: String
)

class ConversationListViewModel(
    private val messageApi: MessageApi,
    private val driverApi: DriverApi,
    private val truckApi: TruckApi,
    private val preferencesManager: PreferencesManager,
    private val conversationStateManager: ConversationStateManager
) : ViewModel() {

    private val _uiState = MutableStateFlow<ConversationListUiState>(ConversationListUiState.Loading)
    val uiState: StateFlow<ConversationListUiState> = _uiState.asStateFlow()

    private val _dispatcherInfo = MutableStateFlow<DispatcherInfo?>(null)
    val dispatcherInfo: StateFlow<DispatcherInfo?> = _dispatcherInfo.asStateFlow()

    private val _createState = MutableStateFlow<CreateConversationState>(CreateConversationState.Idle)
    val createState: StateFlow<CreateConversationState> = _createState.asStateFlow()

    private val _teamChatState = MutableStateFlow<TeamChatState>(TeamChatState.Idle)
    val teamChatState: StateFlow<TeamChatState> = _teamChatState.asStateFlow()

    val unreadCount: StateFlow<Int> = conversationStateManager.unreadCount

    private var currentUserId: String? = null

    init {
        loadConversations()
        loadDispatcherInfo()
        observeConversationUpdates()
    }

    private fun observeConversationUpdates() {
        viewModelScope.launch {
            conversationStateManager.conversationUpdated.collect { conversationId ->
                // Refresh conversations when a new message arrives
                loadConversations(silent = true)
            }
        }
    }

    fun loadConversations(silent: Boolean = false) {
        viewModelScope.launch {
            if (!silent) {
                _uiState.value = ConversationListUiState.Loading
            }

            currentUserId = preferencesManager.getUserId()
            val response = messageApi.getConversations(participantId = currentUserId)

            if (!response.success) {
                _uiState.value = ConversationListUiState.Error(
                    "Failed to load conversations (${response.status})"
                )
                return@launch
            }

            val conversations = response.body()
            _uiState.value = ConversationListUiState.Success(conversations)

            val totalUnread = conversations.sumOf { it.unreadCount ?: 0 }
            conversationStateManager.updateUnreadCount(totalUnread)
        }
    }

    fun refresh() {
        loadConversations()
    }

    private fun loadDispatcherInfo() {
        viewModelScope.launch {
            val userId = preferencesManager.getUserId() ?: return@launch

            val driverResponse = driverApi.getDriverByUserId(userId)
            if (!driverResponse.success) return@launch

            val driver = driverResponse.body()
            val driverId = driver.id ?: return@launch

            val truckResponse = truckApi.getTruckById(
                driverId,
                includeLoads = true,
                onlyActiveLoads = true
            )
            if (!truckResponse.success) return@launch

            val truck = truckResponse.body()
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

        val existingConversation = (_uiState.value as? ConversationListUiState.Success)
            ?.conversations
            ?.find { conversation ->
                conversation.participants?.any { it.employeeId == dispatcher.id } == true
            }

        if (existingConversation != null) {
            _createState.value = CreateConversationState.Success(existingConversation.id!!)
            return
        }

        viewModelScope.launch {
            _createState.value = CreateConversationState.Creating

            val response = messageApi.createConversation(
                CreateConversationRequest(
                    participantIds = listOf(userId, dispatcher.id),
                    name = "Chat with ${dispatcher.name}"
                )
            )

            if (!response.success) {
                _createState.value = CreateConversationState.Error(
                    "Failed to create conversation (${response.status})"
                )
                return@launch
            }

            val conversation = response.body()
            conversation.id?.let { conversationId ->
                loadConversations()
                _createState.value = CreateConversationState.Success(conversationId)
            }
        }
    }

    fun openTeamChat() {
        viewModelScope.launch {
            _teamChatState.value = TeamChatState.Loading

            val response = messageApi.getTenantChat()

            if (!response.success) {
                _teamChatState.value = TeamChatState.Error(
                    "Failed to load team chat (${response.status})"
                )
                return@launch
            }

            val tenantChat = response.body()
            tenantChat.id?.let { conversationId ->
                loadConversations()
                _teamChatState.value = TeamChatState.Success(conversationId)
            }
        }
    }

    fun resetCreateState() {
        _createState.value = CreateConversationState.Idle
    }

    fun resetTeamChatState() {
        _teamChatState.value = TeamChatState.Idle
    }
}

sealed class ConversationListUiState {
    data object Loading : ConversationListUiState()
    data class Success(val conversations: List<ConversationDto>) : ConversationListUiState()
    data class Error(val message: String) : ConversationListUiState()
}

sealed class TeamChatState {
    data object Idle : TeamChatState()
    data object Loading : TeamChatState()
    data class Success(val conversationId: String) : TeamChatState()
    data class Error(val message: String) : TeamChatState()
}
