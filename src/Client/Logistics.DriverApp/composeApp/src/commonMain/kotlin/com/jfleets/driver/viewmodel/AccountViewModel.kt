package com.jfleets.driver.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.UserApi
import com.jfleets.driver.api.models.UpdateUserCommand
import com.jfleets.driver.api.models.UserDto
import com.jfleets.driver.service.PreferencesManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

class AccountViewModel(
    private val userApi: UserApi,
    private val preferencesManager: PreferencesManager
) : ViewModel() {

    private val _uiState = MutableStateFlow<AccountUiState>(AccountUiState.Loading)
    val uiState: StateFlow<AccountUiState> = _uiState.asStateFlow()

    private val _saveState = MutableStateFlow<SaveState>(SaveState.Idle)
    val saveState: StateFlow<SaveState> = _saveState.asStateFlow()

    init {
        loadUser()
    }

    private fun loadUser() {
        viewModelScope.launch {
            _uiState.value = AccountUiState.Loading
            val userId = preferencesManager.getUserId()

            if (userId.isNullOrEmpty()) {
                _uiState.value = AccountUiState.Error("User ID not available")
                return@launch
            }

            try {
                val user = userApi.getUserById(userId).body()
                _uiState.value = AccountUiState.Success(user)
            } catch (e: Exception) {
                _uiState.value = AccountUiState.Error(e.message ?: "An error occurred")
            }
        }
    }

    fun updateUser(user: UserDto) {
        viewModelScope.launch {
            _saveState.value = SaveState.Saving
            try {
                val updateUserCommand = UpdateUserCommand(
                    id = user.id,
                    firstName = user.firstName,
                    lastName = user.lastName,
                    phoneNumber = user.phoneNumber
                )
                userApi.updateUser(user.id!!, updateUserCommand)
                _saveState.value = SaveState.Success
                _uiState.value = AccountUiState.Success(user)
            } catch (e: Exception) {
                _saveState.value = SaveState.Error(e.message ?: "An error occurred")
            }
        }
    }

    fun resetSaveState() {
        _saveState.value = SaveState.Idle
    }
}

sealed class AccountUiState {
    object Loading : AccountUiState()
    data class Success(val user: UserDto) : AccountUiState()
    data class Error(val message: String) : AccountUiState()
}

sealed class SaveState {
    object Idle : SaveState()
    object Saving : SaveState()
    object Success : SaveState()
    data class Error(val message: String) : SaveState()
}
