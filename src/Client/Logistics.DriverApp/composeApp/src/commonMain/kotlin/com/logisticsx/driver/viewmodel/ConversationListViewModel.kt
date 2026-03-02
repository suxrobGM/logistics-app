package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.DriverApi
import com.logisticsx.driver.api.MessageApi
import com.logisticsx.driver.api.TruckApi
import com.logisticsx.driver.api.bodyOrThrow
import com.logisticsx.driver.api.models.ConversationDto
import com.logisticsx.driver.api.models.CreateConversationRequest
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.service.messaging.ConversationStateManager
import com.logisticsx.driver.viewmodel.base.ActionState
import com.logisticsx.driver.viewmodel.base.BaseViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

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
) : BaseViewModel() {

    private val _uiState = MutableStateFlow<UiState<List<ConversationDto>>>(UiState.Loading)
    val uiState: StateFlow<UiState<List<ConversationDto>>> = _uiState.asStateFlow()

    private val _dispatcherInfo = MutableStateFlow<DispatcherInfo?>(null)
    val dispatcherInfo: StateFlow<DispatcherInfo?> = _dispatcherInfo.asStateFlow()

    private val _createState = MutableStateFlow<ActionState<String>>(ActionState.Idle)
    val createState: StateFlow<ActionState<String>> = _createState.asStateFlow()

    private val _teamChatState = MutableStateFlow<ActionState<String>>(ActionState.Idle)
    val teamChatState: StateFlow<ActionState<String>> = _teamChatState.asStateFlow()

    val unreadCount: StateFlow<Int> = conversationStateManager.unreadCount

    private var currentUserId: String? = null

    init {
        loadConversations()
        loadDispatcherInfo()
        observeConversationUpdates()
    }

    private fun observeConversationUpdates() {
        launchSafely {
            conversationStateManager.conversationUpdated.collect {
                loadConversations(silent = true)
            }
        }
    }

    fun loadConversations(silent: Boolean = false) {
        launchSafely(onError = { e ->
            _uiState.value = UiState.Error(e.message ?: "Failed to load conversations")
        }) {
            if (!silent) {
                _uiState.value = UiState.Loading
            }

            currentUserId = preferencesManager.getUserId()
            val conversations = messageApi.getConversations(participantId = currentUserId).bodyOrThrow()
            _uiState.value = UiState.Success(conversations)

            val totalUnread = conversations.sumOf { it.unreadCount ?: 0 }
            conversationStateManager.updateUnreadCount(totalUnread)
        }
    }

    fun refresh() {
        loadConversations()
    }

    private fun loadDispatcherInfo() {
        launchSafely {
            val userId = preferencesManager.getUserId() ?: return@launchSafely

            val driver = driverApi.getDriverByUserId(userId).bodyOrThrow()
            val driverId = driver.id ?: return@launchSafely

            val truck = truckApi.getTruckById(
                driverId,
                includeLoads = true,
                onlyActiveLoads = true
            ).bodyOrThrow()

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

        val existingConversation = (_uiState.value as? UiState.Success)
            ?.data
            ?.find { conversation ->
                conversation.participants?.any { it.employeeId == dispatcher.id } == true
            }

        if (existingConversation != null) {
            _createState.value = ActionState.Success(existingConversation.id!!)
            return
        }

        launchSafely(onError = { e ->
            _createState.value = ActionState.Error(e.message ?: "Failed to create conversation")
        }) {
            _createState.value = ActionState.Loading

            val conversation = messageApi.createConversation(
                CreateConversationRequest(
                    participantIds = listOf(userId, dispatcher.id),
                    name = "Chat with ${dispatcher.name}"
                )
            ).bodyOrThrow()

            conversation.id?.let { conversationId ->
                loadConversations()
                _createState.value = ActionState.Success(conversationId)
            }
        }
    }

    fun openTeamChat() {
        launchSafely(onError = { e ->
            _teamChatState.value = ActionState.Error(e.message ?: "Failed to load team chat")
        }) {
            _teamChatState.value = ActionState.Loading

            val tenantChat = messageApi.getTenantChat().bodyOrThrow()
            tenantChat.id?.let { conversationId ->
                loadConversations()
                _teamChatState.value = ActionState.Success(conversationId)
            }
        }
    }

    fun resetCreateState() {
        _createState.value = ActionState.Idle
    }

    fun resetTeamChatState() {
        _teamChatState.value = ActionState.Idle
    }
}
