package com.jfleets.driver.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.jfleets.driver.api.UserApi
import com.jfleets.driver.data.mapper.toDomain
import com.jfleets.driver.data.mapper.toUpdateCommand
import com.jfleets.driver.model.User
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
                val response = userApi.getUserById(userId)
                val result = response.body()
                if (result.success == true && result.data != null) {
                    _uiState.value = AccountUiState.Success(result.data.toDomain())
                } else {
                    _uiState.value = AccountUiState.Error(result.error ?: "Failed to load user")
                }
            } catch (e: Exception) {
                _uiState.value = AccountUiState.Error(e.message ?: "An error occurred")
            }
        }
    }

    fun updateUser(user: User) {
        viewModelScope.launch {
            _saveState.value = SaveState.Saving
            try {
                val response = userApi.updateUser(user.id, user.toUpdateCommand())
                val result = response.body()
                if (result.success == true) {
                    _saveState.value = SaveState.Success
                    _uiState.value = AccountUiState.Success(user)
                } else {
                    _saveState.value = SaveState.Error(result.error ?: "Failed to update user")
                }
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
    data class Success(val user: User) : AccountUiState()
    data class Error(val message: String) : AccountUiState()
}

sealed class SaveState {
    object Idle : SaveState()
    object Saving : SaveState()
    object Success : SaveState()
    data class Error(val message: String) : SaveState()
}
