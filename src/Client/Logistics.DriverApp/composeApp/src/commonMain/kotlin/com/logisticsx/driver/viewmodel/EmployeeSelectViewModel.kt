package com.logisticsx.driver.viewmodel

import com.logisticsx.driver.api.EmployeeApi
import com.logisticsx.driver.api.MessageApi
import com.logisticsx.driver.api.bodyOrThrow
import com.logisticsx.driver.api.models.CreateConversationRequest
import com.logisticsx.driver.api.models.EmployeeDto
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.viewmodel.base.ActionState
import com.logisticsx.driver.viewmodel.base.BaseViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.FlowPreview
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.debounce
import kotlinx.coroutines.flow.distinctUntilChanged
import kotlinx.coroutines.flow.launchIn
import kotlinx.coroutines.flow.onEach

@OptIn(FlowPreview::class)
class EmployeeSelectViewModel(
    private val messageApi: MessageApi,
    private val employeeApi: EmployeeApi,
    private val preferencesManager: PreferencesManager
) : BaseViewModel() {

    private val _searchQuery = MutableStateFlow("")
    val searchQuery: StateFlow<String> = _searchQuery.asStateFlow()

    private val _searchState = MutableStateFlow<UiState<List<EmployeeDto>>?>(null)
    val searchState: StateFlow<UiState<List<EmployeeDto>>?> = _searchState.asStateFlow()

    private val _createState = MutableStateFlow<ActionState<String>>(ActionState.Idle)
    val createState: StateFlow<ActionState<String>> = _createState.asStateFlow()

    private var currentUserId: String? = null

    init {
        launchSafely {
            currentUserId = preferencesManager.getUserId()
        }

        _searchQuery
            .debounce(300)
            .distinctUntilChanged()
            .onEach { query -> executeSearch(query) }
            .launchIn(viewModelScope)
    }

    fun setSearchQuery(query: String) {
        _searchQuery.value = query
        if (query.length < 2) {
            _searchState.value = null
        }
    }

    fun retrySearch() {
        executeSearch(_searchQuery.value)
    }

    private fun executeSearch(query: String) {
        if (query.length < 2) {
            _searchState.value = null
            return
        }

        launchSafely(onError = { e ->
            _searchState.value = UiState.Error(e.message ?: "Failed to search employees")
        }) {
            _searchState.value = UiState.Loading

            val response = employeeApi.getEmployees(search = query).bodyOrThrow()
            val employees = response?.items ?: emptyList()
            val filteredEmployees = employees.filter { it.id != currentUserId }
            _searchState.value = UiState.Success(filteredEmployees)
        }
    }

    fun startConversationWithEmployee(employee: EmployeeDto) {
        val userId = currentUserId ?: return
        val employeeId = employee.id ?: return

        launchSafely(onError = { e ->
            _createState.value = ActionState.Error(e.message ?: "Failed to create conversation")
        }) {
            _createState.value = ActionState.Loading

            // Check if conversation already exists
            val conversations = messageApi.getConversations(participantId = userId).bodyOrThrow()
            val existingConversation = conversations.find { conversation ->
                conversation.isTenantChat != true &&
                    conversation.participants?.any { it.employeeId == employeeId } == true &&
                    conversation.participants.size == 2
            }

            if (existingConversation != null) {
                _createState.value = ActionState.Success(existingConversation.id!!)
                return@launchSafely
            }

            // Create new conversation
            val conversation = messageApi.createConversation(
                CreateConversationRequest(participantIds = listOf(userId, employeeId))
            ).bodyOrThrow()

            conversation.id?.let { conversationId ->
                _createState.value = ActionState.Success(conversationId)
            }
        }
    }

    fun resetCreateState() {
        _createState.value = ActionState.Idle
    }
}
