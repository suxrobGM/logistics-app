package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.EmployeeApi
import com.jfleets.driver.api.MessageApi
import com.jfleets.driver.api.models.CreateConversationRequest
import com.jfleets.driver.api.models.EmployeeDto
import com.jfleets.driver.service.PreferencesManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class EmployeeSelectViewModel(
    private val messageApi: MessageApi,
    private val employeeApi: EmployeeApi,
    private val preferencesManager: PreferencesManager
) : ViewModel() {

    private val _searchState = MutableStateFlow<EmployeeSearchState>(EmployeeSearchState.Idle)
    val searchState: StateFlow<EmployeeSearchState> = _searchState.asStateFlow()

    private val _createState = MutableStateFlow<CreateConversationState>(CreateConversationState.Idle)
    val createState: StateFlow<CreateConversationState> = _createState.asStateFlow()

    private var currentUserId: String? = null

    init {
        viewModelScope.launch {
            currentUserId = preferencesManager.getUserId()
        }
    }

    fun searchEmployees(query: String) {
        if (query.length < 2) {
            _searchState.value = EmployeeSearchState.Idle
            return
        }

        viewModelScope.launch {
            _searchState.value = EmployeeSearchState.Loading

            val response = employeeApi.getEmployees(search = query, pageSize = 20)

            if (!response.success) {
                _searchState.value = EmployeeSearchState.Error(
                    "Failed to search employees (${response.status})"
                )
                return@launch
            }

            val employees = response.body().items ?: emptyList()
            val filteredEmployees = employees.filter { it.id != currentUserId }
            _searchState.value = EmployeeSearchState.Success(filteredEmployees)
        }
    }

    fun startConversationWithEmployee(employee: EmployeeDto) {
        val userId = currentUserId ?: return
        val employeeId = employee.id ?: return

        viewModelScope.launch {
            _createState.value = CreateConversationState.Creating

            // Check if conversation already exists
            val conversationsResponse = messageApi.getConversations(participantId = userId)
            if (conversationsResponse.success) {
                val existingConversation = conversationsResponse.body().find { conversation ->
                    conversation.isTenantChat != true &&
                        conversation.participants?.any { it.employeeId == employeeId } == true &&
                        conversation.participants.size == 2
                }

                if (existingConversation != null) {
                    _createState.value = CreateConversationState.Success(existingConversation.id!!)
                    return@launch
                }
            }

            // Create new conversation
            val response = messageApi.createConversation(
                CreateConversationRequest(
                    participantIds = listOf(userId, employeeId),
                    name = null
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
                _createState.value = CreateConversationState.Success(conversationId)
            }
        }
    }

    fun clearSearch() {
        _searchState.value = EmployeeSearchState.Idle
    }

    fun resetCreateState() {
        _createState.value = CreateConversationState.Idle
    }
}

sealed class EmployeeSearchState {
    data object Idle : EmployeeSearchState()
    data object Loading : EmployeeSearchState()
    data class Success(val employees: List<EmployeeDto>) : EmployeeSearchState()
    data class Error(val message: String) : EmployeeSearchState()
}

sealed class CreateConversationState {
    data object Idle : CreateConversationState()
    data object Creating : CreateConversationState()
    data class Success(val conversationId: String) : CreateConversationState()
    data class Error(val message: String) : CreateConversationState()
}
